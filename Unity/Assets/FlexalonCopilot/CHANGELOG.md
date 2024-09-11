# Changelog

## Version 1.2.0

### Features
- Copilot will explain its capabilities when creating a new chat log.
- Copilot will now respond to questions.
- Copilot responses can be twice as long.
- The Chat Window has been revamped and has many little quality of life improvements.
- Changed the default prefab set to 'LightPrefabSet', which has been improved. This lets Copilot produce better looking UI by default.
- Significantly improved Copilot's response time.
- Better Slider support: Copilot can assign min, max, value, and color.
- A new chat log is automatically created if there isn't one.
- A new canvas is automatically created if it isn't set.
- The scene view camera will now frame the Canvas when Copilot is making edits.

### Fixes
- Fixed Copilot's first response comment being cut off.
- Fixed multiple issues causing Copilot to fail to set a background image
- Fixed Copilot sometimes failing to set size and other properties.
- Fixed some errors when Copilot would remove and recreate gameObjects.
- Improved error handling from AI service to display proper errors.
- A warning will be displayed if the response length limit is reached.
- Fixed animations getting stuck, causing Copilot to be "processing" forever.
- Added some helpful messages when Copilot fails to assign sizes, fonts, and images.
- Fixed undo not working for Flexalon Styles.

## Version 1.1.0

- Added a button to update the Flexalon Copilot Unity Package if needed.
- Copilot can now display comments for each step of its process.
- Copilot can now display a warning if it can't perform tasks like generate scripts.
- Fixed several bugs in adding or removing scroll views.
- Chat window will now display how your prompt is amended when you select objects.
- Fixed an issue where Copilot would sometimes generate layouts with circular size dependencies, causing the size to be zero.
- Improved prompts asking to 'center' an object on the screen.
- Replaced use of Mask with RectMask2D for scroll view and hiding overflow content.
- Improved prompts asking for a 'brown' or 'bronze' color.
- Fixed a bug where Copilot tries to reduce the gap between objects and fails.
- Fixed some cases where Copilot tries to anchor an object to the edge of the screen.
- Fixed a bug where text which is not in a Flexalon layout would get an incorrect size.
- Fixed an issue with Copilot misinterpreting a Flexalon Grid layout in the scene.
- Allow Copilot to use arbitrary fractions for fill sizes.
- Fixed a bug where the sprite may be removed from buttons when changing the color.
- Automatically compute the required rows/columns when generating a grid layout based on the number of children.

## Version 1.0.0

- Welcome to Flexalon Copilot Early Access!
- For getting started instructions, please visit https://ai.flexalon.com.