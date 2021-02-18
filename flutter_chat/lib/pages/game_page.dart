import 'package:flutter/material.dart';

import 'package:auto_route/auto_route.dart';

import 'package:auto_orientation/auto_orientation.dart';

class GamePage extends StatefulWidget {
  final Orientation orientation;
  GamePage({Key key, @required this.orientation}) : super(key: key);

  @override
  _GamePageState createState() => _GamePageState();
}

class _GamePageState extends State<GamePage> {
  get orientation => orientation;

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: WillPopScope(
          onWillPop: () {
            ExtendedNavigator.root.pop();
            return;
          },
          child: Center(
            child: Container(
              child: Text('GAME'),
            ),
          ),
        ),
      ),
    );
  }
}
