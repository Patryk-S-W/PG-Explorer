import 'package:flutter/material.dart';

import 'package:provider/provider.dart';
import 'package:auto_route/auto_route.dart';
import 'package:connectivity/connectivity.dart';
import 'package:flutter_screenutil/screenutil_init.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'package:flutter_chat/providers/app_language.dart';
import 'package:flutter_chat/router/router.gr.dart' as rt;
import 'package:flutter_chat/services/app_localizations.dart';
import 'package:flutter_chat/services/connectivity_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  AppLanguage appLanguage = AppLanguage();
  await appLanguage.fetchLocale();
  runApp(MyApp(
    appLanguage: appLanguage,
  ));
}

class MyApp extends StatelessWidget {
  final AppLanguage appLanguage;

  MyApp({this.appLanguage});
  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        StreamProvider<ConnectivityResult>(
          create: (_) =>
              ConnectivityService().connectionStatusController.stream,
        ),
        ChangeNotifierProvider<AppLanguage>(create: (_) => appLanguage)
      ],
      child: LayoutBuilder(
        builder: (context, constraints) {
          return OrientationBuilder(
            builder: (builder, orientation) {
              return ScreenUtilInit(
                allowFontScaling: true,
                builder: () => Consumer<AppLanguage>(
                  builder: (context, model, child) {
                    return MaterialApp(
                      debugShowCheckedModeBanner: false,
                      home: Container(),
                      builder: ExtendedNavigator.builder(
                        router: rt.Router(),
                        initialRoute: rt.Routes.homePage,
                        builder: (_, navigator) =>
                            Theme(data: ThemeData.dark(), child: navigator),
                      ),
                      locale: model.appLocal,
                      supportedLocales: [
                        Locale('en', 'US'),
                        Locale('pl', 'PL'),
                      ],
                      localizationsDelegates: [
                        AppLocalizations.delegate,
                        GlobalMaterialLocalizations.delegate,
                        GlobalWidgetsLocalizations.delegate,
                      ],
                    );
                  },
                ),
              );
            },
          );
        },
      ),
    );
  }
}
