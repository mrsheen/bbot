Bejeweled Blitz Bot
============

[mrsheen-bbot]

## Description

A fork of Mark Ashley Bell's C# Bejeweled Bot [mab-bbot] repository, which itself is a port of William Henry's Java Bejeweled Bot [wh-bbot].

Includes many ideas from Charles Cook [cc-bbot], including the use of AForge.NET for image statistics, an algorithm for efficient full-screen searching of game menus, and a slick solution for unecessary moves using a move dampener.

From Mark's [mab-bbot] page

So, this is my attempt at porting William Henry's [Bejeweled Bot]() to C#. It's not very efficient yet, and it doesn't currently match special gems, so it occasionally gets 'stuck'. However, it's still a darn sight faster than I am... :)

## License

No license attribution was attached

## Instructions

 1. Open Facebook, start the Bejeweled Blitz application (navigate to 'Play Now')
 2. Start up the Bejeweled Blitz Bot
 3. (Optionally) Click 'Find in Screen' and drag the mouse from one corner of the game screen to the opposite
 4. Click 'Play', and watch the mouse go crazy!
 5. Press 'Escape' to stop the game at any time
 
 After the game runs for the first time, and finds the location of the game on your screen, the location is cached in a config file on disk (beside the executable). Subsequent runs of the application will try the cached location first, before falling back to a slower pixel-by-pixel match of the entire screen.

## Differences with [mab-bbot]

I forked [mab-bbot] to quickly get up to speed with a working program. I have since implemented a number of significant changes to the code structure.

 - Move dampening to reduce double move attempts while matching animation completes
 - Color matching is more precise, using the AForge.Net image processing libraries
 - Game menus are recognised using a state manager, allowing completely automated progression between games
 - Wide range of debugging tools including dumping images to disk (enabled in code, no UI yet)
 - Broad set of image masks to reduce noise when matching menus
 - Global Win32 hotkey (Escape) to kill the game engine if it starts misbehaving
 

[mab-bbot]: https://github.com/markashleybell/bbot
[wh-bbot]: http://mytopcoder.com/BejeweledBot
[cc-bbot]: http://www.charlesrcook.com/archive/2010/09/05/creating-a-bejeweled-blitz-bot-in-c.aspx

[mrsheen-bbot]: https://github.com/mrsheen/bbot