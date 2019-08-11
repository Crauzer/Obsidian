<table>
  <tbody>
    <tr>
      <td><img width=128 height=128 src="https://i.imgur.com/mRQhyZR.png"></td>
      <td><h1>Obsidian</h1></td>
    </tr>
  </tbody>
</table>

Obsidian is a WAD file editor for League of Legends


## Discord
If you want to talk to me, other developers or people in the community, join my discord server:

<table>
  <tbody>
    <tr>
      <td><img width=64 height=64 src="https://cdn.worldvectorlogo.com/logos/discord.svg"></td>
      <td><h1>https://discord.gg/SUHpgaF</h1></td>
    </tr>
  </tbody>
</table> 

If you like this program, consider supporting me by donating: [![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SSQD35B9ZJZXJ)

## How to use

#### Open
Opens an explorer window from which you can open a WAD file of any version. 
___

#### Save
Opens an explorer window from which you can select where to save your WAD file.
___

#### Import Hashtable
Opens an explorer window from which you can select multiple hashtable files to import. (see [Hashtable](#hashtable))
___

#### Export Hashtable
Opens an explorer window from which you can select where to save all of the currently loaded strings as a hashtable. (see [Hashtable](#hashtable))
___

#### Export All
Opens an explorer window from which you can select where to export **all of the currently displayed entries** (can be used in combination with the Filter).
___

#### Export Selected
Opens an explorer window from which you can select where to export **all of the currently selected entries**
___

#### Create Empty
Creates an empty WAD file
___

#### Create From Directory
Creates a WAD file from the selected directory. This should be used when packing a WAD file for a mod with multiple edited files. Obsidian will correctly import all unhashed names inside of the directory you're creating the WAD from. It will also ignore it's helper file "OBSIDIAN_PACKED_MAPPING" which includes data for packed BIN files.
___

#### Remove
Removes all of the currently selected entries.
___

#### Add File
Opens a window where you can select the file which you want to import and also write it's path in the WAD file **(make sure to replace all \ slashes with /)**. You can also decide whether the file should be compressed;
___

#### Add File Redirection
Opens a window where you can write the path for this WAD entry and also which file it redirects to. When other files such as BIN refer to this WAD entry path League will load the File Redirection. This was used while transfering from the RADS system to the new WAD system. It's unsure whether this would still work.
___

#### Add Folder
Opens an explorer window from which you can select a folder to import. Obsidian will import files from all subdirectories.
___

#### Modify Data
Opens an explorer window from which you can select a file whose data will replace the currently selected entry's data.
___

#### Filter
Can be used to filter the displayed entries.
___

### Hashtable
This is a file which you can use to import your custom hashtables into Obsidian, or also export them.
___

### OBSIDIAN_PACKED_MAPPING
Pretty self explanatory, but this file contains file names for some unhashed BIN files in your extraction directory. This is because sometimes those files are so long that they go over the Windows file character limit which means they can't be saved. This file can be deleted because Obsidian doesn't need it, it's there just for the user. It is also ignored by the [Create From Directory](#create-from-directory) function.
___
