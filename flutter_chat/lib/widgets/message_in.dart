import 'package:flutter/material.dart';

import 'package:flutter_chat/models/message.dart';

import 'package:flutter_screenutil/flutter_screenutil.dart';

class MessageIn extends StatelessWidget {
  final Message message;

  const MessageIn({
    Key key,
    @required this.message,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(7.0.r),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: EdgeInsets.only(left: 6.0.r, bottom: 1.5.r),
            child: Row(
              children: [
                Text(
                  message.username,
                  style: TextStyle(
                    fontSize: 10.0.ssp,
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                SizedBox(
                  width: 6.0.r,
                ),
                Text(
                  '--${message.userID}--',
                  style: TextStyle(
                    fontSize: 9.5.ssp,
                    color: Colors.white.withOpacity(0.6),
                  ),
                )
              ],
            ),
          ),
          Flex(
            direction: Axis.horizontal,
            mainAxisAlignment: MainAxisAlignment.start,
            children: [
              Container(
                padding: EdgeInsets.all(7.0.r),
                constraints: BoxConstraints(
                  maxWidth: 0.7.sw,
                ),
                decoration: BoxDecoration(
                    color: Color.fromRGBO(193, 173, 234, 0.7),
                    borderRadius: BorderRadius.only(
                      bottomLeft: Radius.circular(0.0),
                      bottomRight: Radius.circular(15.0),
                      topLeft: Radius.circular(15.0),
                      topRight: Radius.circular(15.0),
                    )),
                child: message.isImage()
                    ? ClipRRect(
                        borderRadius: BorderRadius.only(
                          bottomLeft: Radius.circular(0.0),
                          bottomRight: Radius.circular(15.0),
                          topLeft: Radius.circular(15.0),
                          topRight: Radius.circular(15.0),
                        ),
                        child: Image.memory(
                          message.image,
                          fit: BoxFit.scaleDown,
                        ),
                      )
                    : Text(
                        message.message,
                        style: TextStyle(color: Colors.white),
                      ),
              ),
            ],
          ),
          Padding(
            padding: EdgeInsets.only(top: 1.5.r),
            child: Text(
              message.time,
              textAlign: TextAlign.left,
              style: TextStyle(
                fontSize: 10.ssp,
                color: Colors.white.withOpacity(0.6),
              ),
            ),
          )
        ],
      ),
    );
  }
}
