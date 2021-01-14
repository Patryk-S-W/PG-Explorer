import 'dart:convert';

import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_chat/constants.dart';
import 'package:flutter_chat/models/message.dart';
import 'package:flutter_chat/widgets/message_in.dart';
import 'package:flutter_chat/widgets/message_out.dart';
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

  @override
  void initState() {
    _textController = TextEditingController();
    _scrollController = ScrollController();
    _socketIO = SocketIOManager().createSocketIO(SERVER_URL, '/',
        socketStatusCallback: (data) => {print('SOCKET STATUS ==> $data')});

    _socketIO.init();

    _socketIO.subscribe('time', (data) {
      this.setState(() => _time = data);
      print(data);
    });

    try {
      _socketIO.connect();
    } catch (err) {
      print('Error $err');
    }

    super.initState();
  }

  @override
  void dispose() {
    _socketIO.disconnect();
    _socketIO.destroy();
    super.dispose();
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
            itemCount: 10,
            itemBuilder: (BuildContext context, int index) {
              return index % 3 == 0
                  ? MessageIn(
                      message: Message(),
                    )
                  : MessageOut(
                      message: Message(),
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
                onPressed: () async {},
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
