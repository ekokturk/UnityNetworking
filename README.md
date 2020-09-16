# UNITY NETWORKING (CHAT/PAWN CONTROL)

A RPC based networking system build on top of Unity's UNET. Application supports moving client's pawn with mouse clicks, chat messages including broadcast, personal and team-based.

## How To Use
- Clients need to connect to an available server which is an option on the main screen to choose between either. (uEcho is used for multiple unity project setup)
- A player pawn will be created for each connected client that has a unique client ID. 
- Right clicking to a point on the screen will move the pawn to that location and movement will be multicast to other clients.
- Typing into the chatbox and clicking the available buttons (Broadcast/Team/Players) on the screen will send the message.
- Server can kick players by clicking on their names.

---

## Screenshots
Client
![Client](/Screenshots/1.PNG)

Server
![Server](/Screenshots/2.PNG)

@Copyright (C) 2019, Eser Kokturk. All Rights Reserved.
