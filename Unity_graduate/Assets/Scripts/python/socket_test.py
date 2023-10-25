from socket import *

serverSock = socket(AF_INET, SOCK_STREAM)
serverSock.bind(('',25001))
serverSock.listen(1)

connectionSock, addr = serverSock.accept()

print('Connection from ',str(addr))

data = connectionSock.recv(1024)
print('Received Data : ', data.decode('utf-8'))

connectionSock.send("Yo I'm Server".encode('utf-8'))
print('Message sent successfully')

serverSock.close()
connectionSock.close()