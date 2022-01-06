# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - not changelogged properly
- Initial version

## [2.0.0] - 2021-10-11
### Removed
- Removed the runInFixedUpdate parameter, as it didn't do anything.
### Added
- Updated the docs to say how to actually run things in FixedUpdate.
- Better overall documentation
### Changed
- Replaced a bunch of different internal logging functions with one good one.

## [2.0.1] - 2021-10-11
### Added
- Minor doc improvements
- Update package.json to have documentation, changelog and license url's.

## [2.0.2] - 2022-06-01
### Fixed
- Made PlayerLoopInterface only clean up the systems added through the interface, rather than all functions. This fixes an incompatibility with the Input System, and probably a bunch of other issues!