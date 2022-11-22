# ShutUpHusky (not really, you're alright)

I like Husky, but I don't like conventional commits until I'm merging to Main.

ShutUpHusky automates your commit messages. Doesn't do it well, but doesn't fail either.

This project should probably be called "ShutUpConventionalCommits", but "ShutUpHusky" is much more catchy.

## Usage

Stage your changes as you normally would, then:

`dotnet run C:\Users\You\SomeRepo\`

## Features

If you've updated `SomeFile.ts`, your commit message will read:

`feat(rand-1234): updated some-file`

If you've updated/created/deleted/renamed a number of files which include a common word:

`feat(rand-9999): common > updated common-file, deleted un-common-file, renamed common-ish-file`

If you're on a branch matching the regex `[a-zA-Z]+\\-[0-9]+` such as TICKET-1234:

`feat(ticket-1234): updated a-file-on-this-branch`

You can scroll through this repo's commits for other examples. All but the first were written by this tool.

## License

MIT
