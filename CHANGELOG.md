# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - not changelogged properly
- Initial version

## [2.0.0] - 2021 October 10
### Removed
- Removed the runInFixedUpdate parameter, as it didn't do anything.
### Added
- Updated the docs to say how to actually run things in FixedUpdate.
- Better overall documentation
### Changed
- Replaced a bunch of different internal logging functions with one good one.

## [2.0.1] - 2021 October 11
### Added
- Minor doc improvements
- Update package.json to have documentation, changelog and license url's.

## [2.0.2] - 2022 January 06
### Fixed
- Made PlayerLoopInterface only clean up the systems added through the interface, rather than all functions. This fixes an incompatibility with the Input System, and probably a bunch of other issues!

## [2.0.3] - 2022 June 09
### Fixed 
- Fixed systems added through InsertSystemBefore not getting cleaned up when playmode was exited.

## [2.0.4] - 2023 November 03
### Removed
- Removed the PlayerLoopQuitChecker Monobehavivour, as all we needed was the Application.quitting event.
### Fixed
- Fixed the PlayerLoopQuitChecker objects were never destroyed in the editor. So in addition to using the Application.quitting event, we will make sure to unsubscribe to the event every time it has been called. That way, no memory should be referred between runs of play mode.
