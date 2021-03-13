-we use files to store and manipulate our data

-open the server app and wait for a request from the client
-open the client and send a playing request to the server

-once the server recieves the client request, server can accept the request or not
	-if server accepts the request it will send a message to the client 
	 containing the chosen category name and level.
	-the client will display this message as messagebox and once the client
         accepts to play the game will start.

-the server will start playing and once server choose wrong letter, server will be denied
 to play and client will take his turn to play.

-keep playing untill either the server or client win the game

-the game result will update the score between server and client at score.txt file.

-if server or client quit the game he will be considered as the loser.