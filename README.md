# Obsidian

Fast and modern League of Legends Wad archive explorer

## Getting started

You can download the [latest release from the "Releases" section](https://github.com/Crauzer/Obsidian/releases).

Make sure to read the **"[Setting up hashtables](#setting-up-hashtables)"** section down below.

### Setting up hashtables

In order to properly use Obsidian, you will have to manually set up the hashtables for it to use. The most reliable and up-to-date source of League hashtables is the [CDragon Data repository](https://github.com/CommunityDragon/Data).

#### Wad hashtables

To unhash chunks in wad files, you will need to download the following hashtable files from the CDragon repository:

- `hashes.game.txt(.{x})`
- `hashes.lcu.txt(.{x})`

In order to use these in Obsidian, click the `Open App Directory` button in the bottom toolbar, this should open up the app directory of Obsidian where you should see a folder called `wad_hashtables`, paste the files that you downloaded earlier in there and click the `Reload Hashtables` button right next to the app directory button.

## Development guide

1. Clone the repository (preferably onto a Windows system)
2. Make sure you're running `node v18.16.0` (specified in `.nvmrc`)
3. Run `pnpm install`
4. Run `pnpm tauri dev`
