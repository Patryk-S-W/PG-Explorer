import 'package:flutter/material.dart';

import 'package:flutter_screenutil/flutter_screenutil.dart';

class CustomAlertDialog extends StatelessWidget {
  const CustomAlertDialog({
    Key key,
    this.title,
    this.contentText,
    this.contentImage,
    this.actions,
  }) : super(key: key);

  final String title;
  final String contentText;
  final Widget contentImage;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10.0)),
      title: Text(
        title,
        textAlign: TextAlign.center,
      ),
      content: Container(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: EdgeInsets.only(top: 4.4.r, bottom: 18.0.r),
              child: contentImage,
            ),
            Text(
              contentText,
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
      actions: actions,
    );
  }
}
