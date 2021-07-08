import socket
import time
import struct
import pynput
from pynput.keyboard import Key,Controller

TCP_IP = '127.0.0.1'
UDP_IP = '127.0.0.1'
TCP_PORT = 6400

UDP_PORT_Receive = 5501
UDP_PORT_Relay1 = 5002
UDP_PORT_Relay2 = 5003

udp_socket_receive = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_socket_relay_2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_socket_relay_1 =socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
keyboard = Controller()

udp_socket_receive.bind(("", UDP_PORT_Receive))
#udp_socket_relay_1.bind(("127.0.0.1", UDP_PORT_Relay1))

#udp_socket_relay_2.bind(("127.0.0.1", UDP_PORT_Relay2))

#mreq = struct.pack("=4sl", socket.inet_aton("127.0.0.1"), socket.INADDR_ANY)
#udp_socket_receive.setsockopt(socket.IPPROTO_IP, socket.IP_ADD_MEMBERSHIP, mreq)

print("Listening  to port #i",UDP_PORT_Receive)
print("Relaying  to port #i",UDP_PORT_Relay1)
print("Relaying  to port #i",UDP_PORT_Relay2)

#tcp_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#tcp_socket.bind((TCP_IP, TCP_PORT))
#tcp_socket.listen(100)
#conn, addr = tcp_socket.accept()

print("Address bound\n");

def relayMarker(msg):
    udp_socket_relay_1.sendto(msg,("127.0.0.1", UDP_PORT_Relay1));
    udp_socket_relay_2.sendto(msg,("127.0.0.1", UDP_PORT_Relay2));

while True:
    dataBytes, addr = udp_socket_receive.recvfrom(1024)
    print("Recieved\n")
#    conn.sendall(data)
    data=int.from_bytes(dataBytes,"big")
    print(data)
    
    relayMarker(dataBytes)
    
    if data== 1:
        keyboard.press('1')
        keyboard.release('1')
    elif data== 2:
        keyboard.press('2')
        keyboard.release('2')
    elif data== 3:
        keyboard.press('3')
        keyboard.release('3')
    elif data== 4:
        keyboard.press('4')
        keyboard.release('4')
    elif data== 5:
        keyboard.press('5')
        keyboard.release('5')
    elif data== 6:
        keyboard.press('6')
        keyboard.release('6')
    elif data== 7:
        keyboard.press('7')
        keyboard.release('7')
    elif data== 8:
        keyboard.press('8')
        keyboard.release('8')
    elif data== 9:
        keyboard.press('9')
        keyboard.release('9')
    elif data== 0:
        keyboard.press('0')
        keyboard.release('0')
    elif data== 11:
        keyboard.press(Key.f1)
        keyboard.release(Key.f1)
    elif data== 12:
        keyboard.press(Key.f2)
        keyboard.release(Key.f2)
    elif data== 13:
        keyboard.press(Key.f3)
        keyboard.release(Key.f3)
    elif data== 14:
        keyboard.press(Key.f4)
        keyboard.release(Key.f4)
    elif data== 15:
        keyboard.press(Key.f5)
        keyboard.release(Key.f5)
    elif data== 16:
        keyboard.press(Key.f6)
        keyboard.release(Key.f6)
    elif data== 17:
        keyboard.press(Key.f7)
        keyboard.release(Key.f7)
    elif data== 18:
        keyboard.press(Key.f8)
        keyboard.release(Key.f8)
    elif data== 19:
        keyboard.press(Key.f9)
        keyboard.release(Key.f9)
    elif data== 20:
        keyboard.press(Key.f10)
        keyboard.release(Key.f10)
        
#conn.close()