import 'package:flutter/material.dart';
import 'package:flutter_chat/models/message.dart';

class MessageOut extends StatelessWidget {
  final Message message;

  const MessageOut({
    Key key,
    @required this.message,
  }) : super(key: key);
  @override
  Widget build(BuildContext context) {
    return Container(
        child: Padding(
      padding:
          const EdgeInsets.only(right: 8.0, left: 75.0, top: 8.0, bottom: 8.0),
      child: ClipRRect(
        borderRadius: BorderRadius.only(
            bottomLeft: Radius.circular(15),
            bottomRight: Radius.circular(0),
            topLeft: Radius.circular(15),
            topRight: Radius.circular(15)),
        child: Container(
          color: Color.fromRGBO(147, 112, 219, 0.7),
          child: Stack(
            children: <Widget>[
              Padding(
                padding: EdgeInsets.only(
                    right: 8.0, left: 8.0, top: 8.0, bottom: 15.0),
                child: Text(
                  message.message,
                ),
              ),
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
