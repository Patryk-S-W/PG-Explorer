import 'package:connectivity/connectivity.dart';
import 'package:flutter/material.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter_chat/router/router.gr.dart' as rt;
import 'package:flutter_chat/services/connectivity_service.dart';
import 'package:provider/provider.dart';

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return StreamProvider<ConnectivityResult>(
      create: (_) => ConnectivityService().connectionStatusController.stream,
      child: MaterialApp(
        debugShowCheckedModeBanner: false,
        home: Container(),
        builder: ExtendedNavigator.builder(
          router: rt.Router(),
          initialRoute: rt.Routes.homePage,
          builder: (_, navigator) =>
              Theme(data: ThemeData.dark(), child: navigator),
        ),
      ),
    );
  }
}
