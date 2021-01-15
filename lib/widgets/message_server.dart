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
          child: message.greeting
              ? Text(
                  message.message + ' user_' + message.userID,
                  style: TextStyle(
                      fontSize: 10, color: Colors.white.withOpacity(0.5)),
                )
              : Text(
                  message.message,
                  style: TextStyle(
                      fontSize: 10, color: Colors.white.withOpacity(0.5)),
                ),
        ),
      ),
    );
  }
}
