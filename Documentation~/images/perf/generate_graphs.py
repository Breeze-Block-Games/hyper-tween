import xml.etree.ElementTree as ET
import json
import re
import matplotlib.pyplot as plt
import numpy as np
import os
import time
import glob
from functools import reduce

# ----------------------------
# Helper: extract balanced JSON
# ----------------------------
def extract_balanced_json(text):
    start_index = text.find('{')
    if start_index == -1:
        return None

    brace_count = 0
    json_chars = []

    for ch in text[start_index:]:
        if ch == '{':
            brace_count += 1
        elif ch == '}':
            brace_count -= 1

        json_chars.append(ch)

        if brace_count == 0:
            break

    if brace_count != 0:
        return None

    return ''.join(json_chars)


# --------------------------------------------------
# Iterate all XML text nodes (text + tail)
# --------------------------------------------------
def iter_all_text_nodes(root):
    for elem in root.iter():
        if elem.text:
            yield elem.text
        if elem.tail:
            yield elem.tail


# --------------------------------------------------
# Extract performance test results from XML
# --------------------------------------------------
def extract_performance_test_results(xml_file):
    print(f"[DEBUG] Parsing XML file: {xml_file}")
    tree = ET.parse(xml_file)
    root = tree.getroot()

    # Only match the start marker; balanced parser does the rest
    pattern = re.compile(r'##performancetestresult2:(\{)', re.DOTALL)

    results = []

    for text in iter_all_text_nodes(root):
        for match in pattern.finditer(text):
            start = match.start(1)
            json_str = extract_balanced_json(text[start:])

            if not json_str:
                print("[DEBUG] Failed to extract balanced JSON")
                continue

            try:
                results.append(json.loads(json_str))
            except json.JSONDecodeError as e:
                print("[DEBUG] JSON decode error:", e)
                print(json_str[:200])

    print(f"[DEBUG] Extracted {len(results)} performance results")
    return results


# --------------------------------------------------
# Unit conversion helpers
# --------------------------------------------------
def convert_to_milliseconds(median, unit):
    if unit == 0:      # nanoseconds
        return median * 1e-6
    elif unit == 1:    # microseconds
        return median * 1e-3
    elif unit == 2:    # milliseconds
        return median
    return median


def sum_median_sample_groups(result):
    total = 0.0
    for group in result.get("SampleGroups", []):
        total += convert_to_milliseconds(
            group.get("Median", 0),
            group.get("Unit", 2)
        )
    return total


# --------------------------------------------------
# Recursive dict helpers
# --------------------------------------------------
def recursive_get(d, keys):
    return reduce(lambda c, k: c.get(k, {}), keys, d)


def recursive_set(d, value, keys):
    if len(keys) == 1:
        d[keys[0]] = value
    else:
        d = d.setdefault(keys[0], {})
        recursive_set(d, value, keys[1:])


# --------------------------------------------------
# Structure extracted results
# --------------------------------------------------
def get_structured_results(results):
    name_pattern = re.compile(r'^BreezeBlockGames.HyperTween.UnityShared.Tests\.(.*)\.(.*)_(.*)\((.*)\)$')
    structured = {}

    for result in results:
        name = result.get("Name")
        if not name:
            continue

        match = name_pattern.match(name)
        if not match:
            continue

        category = match.group(1)
        test_prefix = match.group(2)
        test_suffix = match.group(3)
        count = match.group(4)

        keys = [test_prefix, test_suffix, category, count]
        if recursive_get(structured, keys):
            continue

        value = sum_median_sample_groups(result)
        recursive_set(structured, value, keys)

    print("[DEBUG] Structured results:")
    print(json.dumps(structured, indent=4))
    return structured


# --------------------------------------------------
# Plot results to SVG
# --------------------------------------------------
def plot_structured_results(data, output_dir="svg_plots"):
    print(f"[DEBUG] CWD: {os.getcwd()}")
    print(f"[DEBUG] Output directory: {os.path.abspath(output_dir)}")

    if not data:
        print("[DEBUG] plot_structured_results called with EMPTY data")
        return

    os.makedirs(output_dir, exist_ok=True)

    for root_key, root_value in data.items():
        for second_key, second_value in root_value.items():
            bar_labels = []
            bar_data = []
            x_labels = None
            max_value = 0

            for third_key, third_value in second_value.items():
                values = list(third_value.values())
                if any(v != 0 for v in values):
                    if x_labels is None:
                        x_labels = list(third_value.keys())
                    bar_labels.append(third_key)
                    bar_data.append(values)
                    max_value = max(max_value, max(values))

            if not bar_labels:
                print(f"[DEBUG] No valid bar data for {root_key} / {second_key}")
                continue

            plt.figure(figsize=(10, 5))
            plt.title(f"Performance: {root_key} - {second_key}", fontsize=16)

            x = np.arange(len(x_labels))
            bar_width = 0.12
            offsets = np.linspace(
                -bar_width * len(bar_data) / 2,
                bar_width * len(bar_data) / 2,
                len(bar_data)
            )

            for i, (label, values) in enumerate(zip(bar_labels, bar_data)):
                bars = plt.bar(x + offsets[i], values, bar_width, label=label)
                for bar in bars:
                    h = bar.get_height()
                    if h:
                        plt.annotate(
                            f"{h:.2f}",
                            (bar.get_x() + bar.get_width() / 2, h),
                            xytext=(0, 3),
                            textcoords="offset points",
                            ha="center",
                            va="bottom"
                        )

            plt.xticks(x, x_labels)
            plt.xlabel("Number of Tweens")
            plt.ylabel("Execution Time (ms)")
            plt.ylim(top=max_value * 1.2)
            plt.legend(title="Test Suite")

            # Force unique output each run (debug)
            plt.gcf().text(
                0.99, 0.01,
                f"Generated {time.time()}",
                fontsize=1,
                ha="right",
                va="bottom"
            )

            filename = f"{root_key}_{second_key}.svg".replace(" ", "_").lower()
            filepath = os.path.join(output_dir, filename)

            print(f"[DEBUG] Writing SVG: {filepath}")
            plt.tight_layout()
            plt.savefig(filepath, format="svg")
            plt.close()

            stat = os.stat(filepath)
            print(
                f"[DEBUG] File written | Size: {stat.st_size} bytes | "
                f"Modified: {stat.st_mtime}"
            )

    print(f"[DEBUG] SVG files saved in '{output_dir}'")


# --------------------------------------------------
# Main entry point
# --------------------------------------------------
def main(pattern="TestResults_*.xml"):
    xml_files = sorted(glob.glob(pattern))

    if not xml_files:
        print(f"No XML files found matching pattern: {pattern}")
        return

    print(f"[DEBUG] Found {len(xml_files)} XML files")
    for f in xml_files:
        print(f"[DEBUG]  - {f}")

    all_results = []

    for xml_file in xml_files:
        results = extract_performance_test_results(xml_file)
        print(f"[DEBUG] {xml_file}: {len(results)} results")
        all_results.extend(results)

    print(f"[DEBUG] Total aggregated results: {len(all_results)}")

    if not all_results:
        print("No performance test results found.")
        return

    structured_results = get_structured_results(all_results)
    plot_structured_results(structured_results)



# --------------------------------------------------
# Run
# --------------------------------------------------
if __name__ == "__main__":
    main("TestResults_*.xml")