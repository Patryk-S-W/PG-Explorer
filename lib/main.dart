import 'package:flutter/material.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter_chat/router/router.gr.dart' as rt;

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
        home: Container(),
        builder: ExtendedNavigator.builder(
          router: rt.Router(),
          initialRoute: rt.Routes.homePage,
          builder: (_, navigator) =>
              Theme(data: ThemeData.dark(), child: navigator),
        ));
  }
}
