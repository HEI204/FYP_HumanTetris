## Building a game application for non-intrusive data collection in psychological research

### Human Tetris: Pose Mimicking Game Application
### Overview
Human Tetris is a pose mimicking game application designed for non-intrusive data collection in psychological research. The game uses MediaPipe for real-time body pose tracking through standard cameras and captures detailed body pose data during gameplay. This data can provide insights for psychological analysis, particularly in detecting depressive states.

### Requirements
- Operating System: Windows
- Hardware: Web camera
- Software: UnityMediaPipePlugin, UnityPythonMediaPipeAvatar
- Installation
    - Double-click the "Human Tetris.exe" file to run the installer [<a href="https://drive.google.com/file/d/1eelbEgi3hfWF5EKEYrdn-idK4Ges9RRp/view?usp=sharing">Click here to download</a>].
    - Once the installation is complete, the game will open automatically.

### How to Play
- Start the Game: Launch the game and then select 'Start Game' from the start menu

- Settings: Before starting the game, you can adjust the screen size and display resolution by clicking on the "Setting" button on the main menu.

- BDI-II Questionnaire: You will be prompted to fill in the Beck Depression Inventory-II (BDI-II) questionnaire. This step is optional.
    - If you choose to fill in the questionnaire, your responses will be saved and uploaded along with your pose data after the game.
    - If you choose to skip, you can start the game directly.

### Gameplay:
- Mimic the poses displayed on the approaching walls within the given time.

- You have 4 chances (hearts) to mimic the poses correctly. Each incorrect pose will result in the loss of a heart.

- The game involves 8 distinct poses focusing mainly on upper limb movements.

### Data Upload and Prediction:
After completing the game, your pose data and questionnaire responses will be uploaded to Azure Table Storage if filled the questionnaire.
The game will display your predicted depression score based on the captured data.

### Acknowledgements
Special thanks to Prof. Antoni Chan for guidance and support, and to all the volunteers who participated in game testing and data collection.