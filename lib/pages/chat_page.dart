import 'dart:async';
import 'dart:io';
import 'dart:convert';

import 'package:flutter/material.dart';

import 'package:auto_route/auto_route.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:image_picker/image_picker.dart';
import 'package:flutter_socket_io/socket_io_manager.dart';
import 'package:flutter_socket_io/flutter_socket_io.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:flutter_chat/constants.dart';
import 'package:flutter_chat/models/message.dart';
import 'package:flutter_chat/widgets/message_in.dart';
import 'package:flutter_chat/widgets/message_out.dart';
import 'package:flutter_chat/widgets/message_server.dart';
import 'package:flutter_chat/services/app_localizations.dart';

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
  List<Message> _messages;
  Timer timer;

  @override
  void initState() {
    _messages = List<Message>();
    _textController = TextEditingController();
    _scrollController = ScrollController();
    _socketIO =
        SocketIOManager().createSocketIO(SERVER_URL_HTTPS, SERVER_NAMESPACE,
            socketStatusCallback: (data) => {
                  print('SOCKET STATUS ==> $data'),
                });

    _socketIO.init();

    _socketIO.subscribe('greeting', (jsonData) {
      print('==> $jsonData');
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

    _socketIO.connect();

    Future.delayed(
        Duration(milliseconds: 5),
        () => _socketIO.subscribe('userID', (data) {
              this.setState(() => _userid = data);
              _socketIO.unSubscribe('userID');
            }));

    Future.delayed(
        Duration(milliseconds: 10),
        () => _socketIO.sendMessage(
            'test', json.encode({"username": widget.username})));

    timer = Timer.periodic(
        Duration(seconds: 45),
        (Timer t) => {
              _messages.length > 0
                  ? setState(() => _messages.removeAt(0))
                  : null,
            });
    super.initState();
  }

  @override
  void dispose() {
    _socketIO.disconnect();
    _socketIO.destroy();
    _textController.dispose();
    timer?.cancel();
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
                  : _messages[index].isGreeting
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
      height: 40.0.r,
      color: Colors.grey[900],
      child: Padding(
          padding: EdgeInsets.only(left: 6.0.r),
          child: TextField(
            maxLines: 20,
            controller: _textController,
            decoration: InputDecoration(
              prefixIcon: Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                mainAxisSize: MainAxisSize.min,
                children: [
                  IconButton(
                    icon: Icon(Icons.camera_alt),
                    onPressed: () async {
                      FocusScope.of(context).unfocus();
                      File image = await ImagePicker.pickImage(
                        source: ImageSource.camera,
                      );
                      List<int> imageBytes = image.readAsBytesSync();
                      base64Encode(imageBytes);

                      if (image != null) {
                        Message msg = Message(
                          image: imageBytes,
                          time: _time,
                          userID: _userid,
                          username: widget.username,
                        );
                        _socketIO.sendMessage(
                            'send_message', json.encode(msg.toJson()));
                        this.setState(() => _messages.add(msg));
                        scrollDown();
                      }
                    },
                  ),
                  IconButton(
                    icon: Icon(Icons.image),
                    onPressed: () async {
                      File image = await ImagePicker.pickImage(
                        source: ImageSource.gallery,
                        imageQuality: 50,
                      );
                      List<int> imageBytes = image.readAsBytesSync();
                      base64Encode(imageBytes);

                      if (image != null) {
                        Message msg = Message(
                          image: imageBytes,
                          time: _time,
                          userID: _userid,
                          username: widget.username,
                        );
                        _socketIO.sendMessage(
                            'send_message', json.encode(msg.toJson()));
                        this.setState(() => _messages.add(msg));
                        scrollDown();
                      }
                    },
                  ),
                ],
              ),
              suffixIcon: IconButton(
                icon: Icon(Icons.send),
                onPressed: () async {
                  if (_textController.text.isNotEmpty) {
                    Message msg = Message(
                      message: _textController.text,
                      time: _time,
                      userID: _userid,
                      username: widget.username,
                    );

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
              hintText:
                  AppLocalizations.of(context).translate('Send_a_message'),
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
              _socketIO.unSubscribesAll();
              Future.delayed(Duration(milliseconds: 1),
                  () => ExtendedNavigator.root.pop());
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
