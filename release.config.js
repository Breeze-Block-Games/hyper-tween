const branch =
  (process.env.GITHUB_REF_NAME ||
   process.env.CI_COMMIT_BRANCH ||
   process.env.BRANCH_NAME ||
   '').toLowerCase();

const isMain = branch === 'main';

module.exports = {
  branches: [
    'main',
    { name: '*', prerelease: '${name}' }
  ],

  plugins: [
    // Always needed to compute the next version and notes
    '@semantic-release/commit-analyzer',
    '@semantic-release/release-notes-generator',
    'semantic-release-export-data',

    // === MAIN: full release path ===
    ...(isMain ? [
      ['@semantic-release/changelog', { changelogFile: 'CHANGELOG.md' }],
      ['@semantic-release/npm', { npmPublish: false }],
      ['@semantic-release/git', { assets: ['CHANGELOG.md', 'package.json'] }],
      [
		'@semantic-release/github',
		{
		  successComment: false,
		  failComment: false,
		  releasedLabels: false
		}
	  ],
    ] : [])
  ],
};
