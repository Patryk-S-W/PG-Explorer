import 'dart:async';
import 'dart:math' as math;
import 'package:connectivity/connectivity.dart';
import 'package:flutter/material.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_chat/router/router.gr.dart';
import 'package:flutter_chat/widgets/custom_alert_dialog.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:provider/provider.dart';

class HomePage extends StatefulWidget {
  HomePage({Key key}) : super(key: key);

  @override
  _HomePageState createState() => _HomePageState();
}

class _HomePageState extends State<HomePage>
    with SingleTickerProviderStateMixin {
  AnimationController _animationController;
  Animation _animation;
  bool isAlertboxOpened;

  void checkConnectivity() async {
    var connectivityResult = Provider.of<ConnectivityResult>(context);
    var conn = getConnectionValue(connectivityResult);

    Future.delayed(Duration(milliseconds: 10), () {
      if (!isAlertboxOpened && conn == 'None')
        SchedulerBinding.instance.addPostFrameCallback((_) {
          if (!isAlertboxOpened) _showAlert(context);
        });
      else if (isAlertboxOpened && (conn == 'Mobile' || conn == 'Wi-Fi'))
        SchedulerBinding.instance.addPostFrameCallback((_) {
          if (isAlertboxOpened) {
            setState(() => isAlertboxOpened = false);
            Navigator.of(context).pop();
          }
        });
    });
  }

  String getConnectionValue(var connectivityResult) {
    String status = '';
    switch (connectivityResult) {
      case ConnectivityResult.mobile:
        status = 'Mobile';
        break;
      case ConnectivityResult.wifi:
        status = 'Wi-Fi';
        break;
      case ConnectivityResult.none:
        status = 'None';
        break;
      default:
        status = 'None';
        break;
    }
    return status;
  }

  void _showAlert(BuildContext context) async {
    setState(() => isAlertboxOpened = true);
    return showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => CustomAlertDialog(
        title: "No internet connection",
        contentImage: SvgPicture.asset(
          'assets/images/disconnected.svg',
          color: Colors.red,
        ),
        contentText: "Please check your internet connection.",
      ),
    );
  }

  @override
  void initState() {
    isAlertboxOpened = false;
    _animationController =
        AnimationController(vsync: this, duration: Duration(seconds: 2));
    _animationController.repeat(reverse: true);
    _animation = Tween(begin: 1.0, end: 5.0).animate(_animationController)
      ..addListener(() {
        setState(() {});
      });
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    checkConnectivity();

    double _height = MediaQuery.of(context).size.height;
    double _width = MediaQuery.of(context).size.width;

    return Scaffold(
      body: Stack(
        alignment: Alignment.center,
        children: [
          ClipPath(
            child: Container(
              width: _width,
              height: _height,
              color: Colors.black,
            ),
            clipper: TopTriangle(),
          ),
          ClipPath(
            child: Container(
                width: _width,
                height: _height,
                color: Colors.white.withOpacity(0.85)),
            clipper: BottomTriangle(),
          ),
          Column(
            children: [
              Expanded(
                flex: 3,
                child: Center(
                  child: Container(
                    width: 300,
                    child: Image(image: AssetImage('assets/images/logo.png')),
                  ),
                ),
              ),
              Expanded(
                flex: 2,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Container(
                      height: 50.0,
                      margin: EdgeInsets.all(10.0),
                      decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(10.0),
                          boxShadow: [
                            BoxShadow(
                              color: Colors.redAccent,
                              blurRadius: _animation.value,
                              spreadRadius: _animation.value,
                            )
                          ]),
                      child: RaisedButton(
                        onPressed: () {
                          // todo
                        },
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10.0)),
                        padding: EdgeInsets.all(0.0),
                        child: Ink(
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: [Colors.red, Colors.black],
                              begin: Alignment.centerLeft,
                              end: Alignment.centerRight,
                              transform: GradientRotation(math.pi / 4),
                            ),
                            borderRadius: BorderRadius.circular(10.0),
                          ),
                          child: Container(
                            constraints: BoxConstraints(
                                maxWidth: 300.0, minHeight: 50.0),
                            alignment: Alignment.center,
                            child: Text(
                              "PLAY",
                              textAlign: TextAlign.center,
                              style: TextStyle(color: Colors.white),
                            ),
                          ),
                        ),
                      ),
                    ),
                    Container(
                      height: 50.0,
                      margin: EdgeInsets.all(10),
                      child: RaisedButton(
                        onPressed: () {
                          ExtendedNavigator.root.push(Routes.loginChatPage);
                        },
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(10.0),
                        ),
                        padding: EdgeInsets.all(0.0),
                        child: Ink(
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: [Colors.white, Colors.black],
                              begin: Alignment.centerLeft,
                              end: Alignment.centerRight,
                              transform: GradientRotation(math.pi / 4),
                            ),
                            borderRadius: BorderRadius.circular(10.0),
                          ),
                          child: Container(
                            constraints: BoxConstraints(
                                maxWidth: 300.0, minHeight: 50.0),
                            alignment: Alignment.center,
                            child: Text(
                              "CHAT",
                              textAlign: TextAlign.center,
                              style: TextStyle(color: Colors.white),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
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

@override
Widget build(BuildContext context) {
  // TODO: implement build
  throw UnimplementedError();
}
