import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter_chat/constants.dart';
import 'package:flutter_chat/models/message.dart';
import 'package:flutter_chat/widgets/message_in.dart';
import 'package:flutter_chat/widgets/message_out.dart';
import 'package:flutter_chat/widgets/message_server.dart';
import 'package:flutter_socket_io/flutter_socket_io.dart';
import 'package:flutter_socket_io/socket_io_manager.dart';
import 'package:google_fonts/google_fonts.dart';

class ChatPage extends StatefulWidget {
  final String username;

  ChatPage({
    Key key,
    @required this.username,
  }) : super(key: key);

  @override
  _ChatPageState createState() => _ChatPageState();
}

class _ChatPageState extends State<ChatPage> {
  TextEditingController _textController;
  ScrollController _scrollController;
  SocketIO _socketIO;
  String _time;
  String _userid;
  String _connection_status;
  List<Message> _messages;

  @override
  void initState() {
    _messages = List<Message>();
    _textController = TextEditingController();
    _scrollController = ScrollController();
    _socketIO = SocketIOManager().createSocketIO(SERVER_URL, SERVER_NAMESPACE,
        socketStatusCallback: (data) => {print('SOCKET STATUS ==> $data')});

    _socketIO.init();

    _socketIO.subscribe('greeting', (jsonData) {
      Message data = Message.fromJson(json.decode(jsonData.toString()));
      this.setState(() => _messages.add(data));
      scrollDown();
    });

    _socketIO.subscribe('receive_message', (jsonData) {
      Message data = Message.fromJson(json.decode(jsonData.toString()));
      this.setState(() => _messages.add(data));
      scrollDown();
    });

    _socketIO.subscribe('time', (data) {
      this.setState(() => _time = data);
    });

    try {
      _socketIO.connect();
    } catch (err) {
      print('Error $err');
    }

    _socketIO.subscribe('userID', (data) {
      this.setState(() => _userid = data);
      _socketIO.unSubscribe('userID');
    });

    Future.delayed(
        Duration(milliseconds: 10),
        () => _socketIO.sendMessage(
            'test', json.encode({"username": widget.username})));

    super.initState();
  }

  @override
  void dispose() {
    _socketIO.disconnect();
    _socketIO.destroy();
    super.dispose();
  }

  void scrollDown() {
    _scrollController.animateTo(
      _scrollController.position.maxScrollExtent,
      duration: Duration(milliseconds: 600),
      curve: Curves.ease,
    );
  }

  Widget messageArea() {
    return Flexible(
      fit: FlexFit.tight,
      child: GestureDetector(
        onTap: () {
          FocusScope.of(context).unfocus();
        },
        child: Container(
          width: MediaQuery.of(context).size.width,
          child: ListView.builder(
            controller: _scrollController,
            itemCount: _messages.length,
            itemBuilder: (BuildContext context, int index) {
              return _messages.length <= 0
                  ? Container()
                  : _messages[index].greeting
                      ? MessageServer(
                          message: _messages[index],
                        )
                      : _messages[index].isUserMessage(_userid)
                          ? MessageOut(
                              message: _messages[index],
                            )
                          : MessageIn(
                              message: _messages[index],
                            );
            },
          ),
        ),
      ),
    );
  }

  Widget inputArea() {
    return Container(
      height: 50,
      color: Colors.grey[900],
      child: Padding(
          padding: EdgeInsets.only(left: 8.0),
          child: TextField(
            maxLines: 20,
            controller: _textController,
            decoration: InputDecoration(
              suffixIcon: IconButton(
                icon: Icon(Icons.send),
                onPressed: () async {
                  if (_textController.text.isNotEmpty) {
                    Message msg = Message(
                        message: _textController.text,
                        time: _time,
                        userID: _userid,
                        username: widget.username);

                    _socketIO.sendMessage(
                        'send_message', json.encode(msg.toJson()));
                    this.setState(() => _messages.add(msg));
                    Future.delayed(Duration(microseconds: 1),
                        () => _textController.clear());
                    scrollDown();
                  }
                },
              ),
              border: InputBorder.none,
              hintText: 'Send a message...',
            ),
          )),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(
          elevation: 1,
          iconTheme: IconThemeData(
            color: Colors.white,
          ),
          centerTitle: true,
          title: Text(
            'LeChat',
            style: GoogleFonts.rockSalt(
              textStyle: TextStyle(
                color: Colors.white,
                fontSize: 18,
              ),
            ),
          ),
          leading: IconButton(
            icon: Icon(Icons.arrow_back_ios),
            onPressed: () {
              Future.delayed(Duration.zero, () => ExtendedNavigator.root.pop());
            },
          ),
        ),
        body: Column(
          children: [
            Divider(
              height: 0,
              color: Colors.black54,
            ),
            messageArea(),
            Divider(height: 0, color: Colors.black26),
            inputArea(),
          ],
        ));
  }
}
