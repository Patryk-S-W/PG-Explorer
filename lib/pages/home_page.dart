import 'dart:async';
import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';

import 'package:provider/provider.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:connectivity/connectivity.dart';
import 'package:flutter_chat/router/router.gr.dart';
import 'package:auto_orientation/auto_orientation.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:flutter_chat/providers/app_language.dart';
import 'package:flutter_chat/widgets/gradient_button.dart';
import 'package:flutter_chat/services/app_localizations.dart';
import 'package:flutter_chat/widgets/custom_alert_dialog.dart';
import 'package:flutter_chat/widgets/triangle_background.dart';

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
        title: AppLocalizations.of(context).translate("No_internet_connection"),
        contentImage: SvgPicture.asset(
          'assets/images/disconnected.svg',
          color: Colors.red,
        ),
        contentText: AppLocalizations.of(context)
            .translate("Please_check_your_internet_connection"),
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
    AppLanguage appLanguage = Provider.of<AppLanguage>(context);
    return Scaffold(
        body: OrientationBuilder(
      builder: (context, orientation) => Stack(
        alignment: Alignment.center,
        children: [
          TriangleBackground(
              color1: Colors.black, color2: Colors.white.withOpacity(0.85)),
          Column(
            children: [
              Expanded(
                flex: 3,
                child: Center(
                  child: Container(
                    width: 260.0.r,
                    child: Image(image: AssetImage('assets/images/logo.png')),
                  ),
                ),
              ),
              Expanded(
                flex: 2,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    GradientButton(
                      child: Text(
                        AppLocalizations.of(context).translate('PLAY'),
                        textAlign: TextAlign.center,
                        style: TextStyle(color: Colors.white),
                      ),
                      transform: GradientRotation(math.pi / 4),
                      colors: [Colors.red, Colors.black],
                      decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(10.0),
                          boxShadow: [
                            BoxShadow(
                              color: Colors.redAccent,
                              blurRadius: _animation.value,
                              spreadRadius: _animation.value,
                            )
                          ]),
                      onPressed: () {
                        ExtendedNavigator.root.push(Routes.gamePage,
                            arguments:
                                GamePageArguments(orientation: orientation));
                      },
                    ),
                    GradientButton(
                      child: Text(
                        AppLocalizations.of(context).translate('CHAT'),
                        textAlign: TextAlign.center,
                        style: TextStyle(color: Colors.white),
                      ),
                      transform: GradientRotation(math.pi / 4),
                      colors: [Colors.white, Colors.black],
                      onPressed: () {
                        ExtendedNavigator.root.push(Routes.loginChatPage);
                      },
                    ),
                  ],
                ),
              ),
            ],
          ),
          Align(
            alignment: Alignment.bottomRight,
            child: PopupMenuButton<String>(
              padding: EdgeInsets.all(17.r),
              icon: Icon(Icons.translate_outlined, color: Colors.black),
              onSelected: (String value) {
                switch (value) {
                  case 'English':
                    appLanguage.changeLanguage(Locale("en"));
                    break;

                  case 'Polish':
                    appLanguage.changeLanguage(Locale("pl"));
                    break;
                }
              },
              itemBuilder: (BuildContext context) {
                return ['English', 'Polish']
                    .map((String choice) => PopupMenuItem<String>(
                          value: choice,
                          child: Text(
                              AppLocalizations.of(context).translate(choice)),
                        ))
                    .toList();
              },
            ),
          ),
          Align(
            alignment: Alignment.bottomLeft,
            child: IconButton(
              padding: EdgeInsets.all(17.r),
              icon: Icon(
                Icons.screen_rotation_outlined,
                color: orientation == Orientation.portrait
                    ? Colors.black
                    : Colors.white,
              ),
              onPressed: () {
                orientation == Orientation.portrait
                    ? AutoOrientation.landscapeAutoMode()
                    : AutoOrientation.portraitAutoMode();
              },
            ),
          )
        ],
      ),
    ));
  }
}
