import 'package:flutter/cupertino.dart';

import 'package:flutter_screenutil/flutter_screenutil.dart';

class TriangleBackground extends StatelessWidget {
  final Color color1;
  final Color color2;

  const TriangleBackground({
    Key key,
    @required this.color1,
    @required this.color2,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        ClipPath(
          child: Container(
            width: 1.sw,
            height: 1.sh,
            color: color1,
          ),
          clipper: TopTriangle(),
        ),
        ClipPath(
          child: Container(width: 1.sw, height: 1.sh, color: color2),
          clipper: BottomTriangle(),
        ),
      ],
    );
  }
}

class TopTriangle extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    Path path = Path();
    path.lineTo(0.0, size.height);
    path.lineTo(size.width, 0.0);
    path.close();
    return path;
  }

  @override
  bool shouldReclip(CustomClipper<Path> oldClipper) => false;
}

class BottomTriangle extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    Path path = Path();
    path.moveTo(size.width, 0.0);
    path.lineTo(0.0, size.height);
    path.lineTo(size.width, size.height);
    path.close();
    return path;
  }

  @override
  bool shouldReclip(CustomClipper<Path> oldClipper) => false;
}
