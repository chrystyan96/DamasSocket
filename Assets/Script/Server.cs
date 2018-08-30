using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine;

public class Server : MonoBehaviour {

	public int port;

	private List<ServerClient> clients;
	private List<ServerClient> disconnetList;

	private TcpListener server;
	private bool serverStarted;

	public void Init() {
		DontDestroyOnLoad(gameObject);
		port = 6321;
		clients = new List<ServerClient>();
		disconnetList = new List<ServerClient>();

		try {
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			StartListening();
			serverStarted = true;
		} catch (Exception e) {
			Debug.LogError("Error! " + e.Message);
		}

	}

	private void Update() {
		if(!serverStarted) 
			return;

		foreach(ServerClient sc in clients) {
			//is client still connected?
			if(!IsConnected(sc.tcp)) {
				sc.tcp.Close();
				disconnetList.Add(sc);
				continue;
			} else {
				NetworkStream netStream = sc.tcp.GetStream();

				if(netStream.DataAvailable) {
					StreamReader reader = new StreamReader(netStream, true);
					string data = reader.ReadLine();

					if(data != null) {
						OnCommingData(sc, data);
					}
				}
			}
		}

		for(int i = 0; i < disconnetList.Count - 1; i++) {
			//tell player when someone desconnected
			clients.Remove(disconnetList[i]);
			disconnetList.RemoveAt(i);
		}
	}

	//server send
	private void Broadcast(string data, List<ServerClient> sc) {
		foreach(ServerClient client in sc) {
			try {
				StreamWriter writer = new StreamWriter(client.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			} catch (Exception e) {
				Debug.Log("Write error : " + e.Message);
			}
		}
	}

	//server read 
	private void OnCommingData(ServerClient sc, string data) {
		Debug.Log(sc.clientName + " : " + data);
	}

	private void StartListening() {
		server.BeginAcceptTcpClient(AcceptTcpClient, server);
	}

	private void AcceptTcpClient(IAsyncResult ar) {
		TcpListener listener = (TcpListener)ar.AsyncState;

		ServerClient client = new ServerClient(listener.EndAcceptTcpClient(ar));
		clients.Add(client);

		StartListening();

		Debug.Log("Somebody has connected!");
	}

	private bool IsConnected(TcpClient tcpClient) {
		try {
			if(tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected) {
				if(tcpClient.Client.Poll(0, SelectMode.SelectRead)) {
					return !(tcpClient.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
				}
				return true;
			} else {
				return false;
			}
		} catch (Exception e) {
			return false;
		}
	}
}

public class ServerClient {
	public string clientName;
	public TcpClient tcp;

	public ServerClient(TcpClient tcp) {
		this.tcp = tcp;
	}
}