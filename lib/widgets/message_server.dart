import 'package:flutter/material.dart';
import 'package:flutter_chat/models/message.dart';

class MessageServer extends StatelessWidget {
  final Message message;

  const MessageServer({
    Key key,
    @required this.message,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: EdgeInsets.all(9.5),
        child: Container(
          child: message.isGreeting
              ? Column(
                  children: [
                    Text(
                      message.username + ' ' + message.message,
                      style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 11,
                          color: Colors.white.withOpacity(0.7)),
                    ),
                    Text(
                      '--${message.userID}--',
                      style: TextStyle(
                          fontSize: 9, color: Colors.white.withOpacity(0.5)),
                    ),
                  ],
                )
              : Container(),
        ),
      ),
    );
  }
}
