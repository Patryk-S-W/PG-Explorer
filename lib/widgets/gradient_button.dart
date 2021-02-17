import 'package:auto_route/auto_route.dart';

import 'package:flutter/material.dart';

import 'package:flutter_screenutil/flutter_screenutil.dart';

class GradientButton extends StatelessWidget {
  final Widget child;
  final GradientRotation transform;
  final List<Color> colors;
  final BoxDecoration decoration;
  final onPressed;

  const GradientButton({
    Key key,
    @required this.child,
    @required this.transform,
    @required this.colors,
    @required this.onPressed,
    this.decoration,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 45.0.r,
      margin: EdgeInsets.all(10),
      decoration: decoration,
      child: RaisedButton(
        onPressed: onPressed,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(10.0),
        ),
        padding: EdgeInsets.all(0.0),
        child: Ink(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: colors,
              begin: Alignment.centerLeft,
              end: Alignment.centerRight,
              transform: transform,
            ),
            borderRadius: BorderRadius.circular(10.0),
          ),
          child: Container(
              constraints: BoxConstraints(maxWidth: 260.0.r, minHeight: 45.0.r),
              alignment: Alignment.center,
              child: child),
        ),
      ),
    );
  }
}
