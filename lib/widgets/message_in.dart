import 'package:flutter/material.dart';
import 'package:flutter_chat/models/message.dart';

class MessageIn extends StatelessWidget {
  final Message message;

  const MessageIn({
    Key key,
    @required this.message,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
        child: Padding(
      padding:
          const EdgeInsets.only(right: 75.0, left: 8.0, top: 8.0, bottom: 8.0),
      child: ClipRRect(
        borderRadius: BorderRadius.only(
            bottomLeft: Radius.circular(0),
            bottomRight: Radius.circular(15),
            topLeft: Radius.circular(15),
            topRight: Radius.circular(15)),
        child: Container(
          color: Color.fromRGBO(193, 173, 234, 0.7),
          child: Stack(
            children: <Widget>[
              Positioned(
                top: 1,
                left: 15,
                child: Row(
                  children: [
                    Text(
                      '${message.userName}  ',
                      style: TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.bold,
                          color: Colors.black.withOpacity(0.6)),
                    ),
                    Text(
                      message.userID ?? '(user_${message.userID})',
                      style: TextStyle(
                          fontSize: 10,
                          fontStyle: FontStyle.italic,
                          color: Colors.black.withOpacity(0.6)),
                    ),
                  ],
                ),
              ),
              Padding(
                  padding: const EdgeInsets.only(
                      right: 8.0, left: 8.0, top: 15.0, bottom: 15.0),
                  child: Text(
                    message.message,
                  )),
              Positioned(
                bottom: 1,
                right: 10,
                child: Text(
                  message.time,
                  style: TextStyle(
                      fontSize: 10, color: Colors.black.withOpacity(0.6)),
                ),
              )
            ],
          ),
        ),
      ),
    ));
  }
}
