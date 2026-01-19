# GitHub Workflows

The default package workflow has the following jobs:

## Semantic Release

Uses [semantic-release](https://github.com/semantic-release/semantic-release) to compute the [semantic version](https://semver.org/)
for each commit for use in other jobs. On the `main` branch it also does the following:

* Writes changes to `CHANGELOG.md`
* Writes the semantic version to `package.json`
* Commits the above files to git and tags the commit with the semantic version
* Creates a GitHub release

# Build Documentation

Builds the documentation you are currently reading using DocFX and syncs to a Google Cloud directory prefixed with the
semantic version. On `main`, it also syncs to a directory prefixed with `latest`.

# Build Service

Uses `Service~/Dockerfile` to build a docker image and pushes it to a Google Cloud repository tagged with the 7 character
SHA of the current commit