# ShutUpHusky (not really, you're alright)

I like Husky, but I don't like conventional commits until I'm merging to Main.

ShutUpHusky automates your commit messages. Doesn't do it well, but doesn't fail either.

This project should probably be called "ShutUpConventionalCommits", but "ShutUpHusky" is much more catchy.

## Usage

Stage your changes as you normally would, then:

`dotnet run -- --repo C:\Users\You\SomeRepo\`

If you want to commit directly without waiting for hooks to run:

`dotnet run -- --repo C:\Users\You\SomeRepo\ --in-process true --name "Your Name" --email "you@mail.com"`

Less meaningful commit message snippets are discarded if the commit title would exceed 72 characters.  
If you want to include discarded commit message snippets in the body:

`dotnet run -- --repo C:\Users\You\SomeRepo\ --enable-body true`

## Features

If you've updated `SomeFile.ts`, your commit message will read:

`chore: updated some-file`

If you've updated/created/deleted/renamed a number of files which include a common word:

`chore: common > updated common-file, deleted un-common-file, renamed common-ish-file`

If you're on a branch matching the regex `[a-zA-Z]+\-[0-9]+` such as TICKET-1234:

`feat(ticket-1234): updated a-file-on-this-branch`

If you have any of the common conventional commit types in your branch name, such as branch/fix/TICKET-4321:

`fix(ticket-4321): updated a-file-on-this-branch`

If you've mostly been changing tests:

`test: updated some-test`

If you've mostly been changing yaml files:

`ci: updated dev3-deployment`

If you've mostly been changing md files:

`docs: updated readme`

You can scroll through this repo's commits for other examples. All but the first were written by this tool.

Commits prefixed with `feat(rand-1234):` are from a previous version, later versions do not generate random scopes.

## License

MIT
