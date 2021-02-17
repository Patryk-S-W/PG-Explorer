// GENERATED CODE - DO NOT MODIFY BY HAND

// **************************************************************************
// AutoRouteGenerator
// **************************************************************************

// ignore_for_file: public_member_api_docs

import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';

import '../pages/chat_page.dart';
import '../pages/game_page.dart';
import '../pages/home_page.dart';
import '../pages/login_chat_page.dart';

class Routes {
  static const String homePage = '/';
  static const String loginChatPage = '/login-chat-page';
  static const String chatPage = '/chat-page';
  static const String gamePage = '/game-page';
  static const all = <String>{
    homePage,
    loginChatPage,
    chatPage,
    gamePage,
  };
}

class Router extends RouterBase {
  @override
  List<RouteDef> get routes => _routes;
  final _routes = <RouteDef>[
    RouteDef(Routes.homePage, page: HomePage),
    RouteDef(Routes.loginChatPage, page: LoginChatPage),
    RouteDef(Routes.chatPage, page: ChatPage),
    RouteDef(Routes.gamePage, page: GamePage),
  ];
  @override
  Map<Type, AutoRouteFactory> get pagesMap => _pagesMap;
  final _pagesMap = <Type, AutoRouteFactory>{
    HomePage: (data) {
      final args = data.getArgs<HomePageArguments>(
        orElse: () => HomePageArguments(),
      );
      return buildAdaptivePageRoute<dynamic>(
        builder: (context) => HomePage(key: args.key),
        settings: data,
      );
    },
    LoginChatPage: (data) {
      final args = data.getArgs<LoginChatPageArguments>(
        orElse: () => LoginChatPageArguments(),
      );
      return buildAdaptivePageRoute<dynamic>(
        builder: (context) => LoginChatPage(key: args.key),
        settings: data,
      );
    },
    ChatPage: (data) {
      final args = data.getArgs<ChatPageArguments>(nullOk: false);
      return buildAdaptivePageRoute<dynamic>(
        builder: (context) => ChatPage(
          key: args.key,
          username: args.username,
        ),
        settings: data,
      );
    },
    GamePage: (data) {
      final args = data.getArgs<GamePageArguments>(nullOk: false);
      return buildAdaptivePageRoute<dynamic>(
        builder: (context) => GamePage(
          key: args.key,
          orientation: args.orientation,
        ),
        settings: data,
      );
    },
  };
}

/// ************************************************************************
/// Arguments holder classes
/// *************************************************************************

/// HomePage arguments holder class
class HomePageArguments {
  final Key key;
  HomePageArguments({this.key});
}

/// LoginChatPage arguments holder class
class LoginChatPageArguments {
  final Key key;
  LoginChatPageArguments({this.key});
}

/// ChatPage arguments holder class
class ChatPageArguments {
  final Key key;
  final String username;
  ChatPageArguments({this.key, @required this.username});
}

/// GamePage arguments holder class
class GamePageArguments {
  final Key key;
  final Orientation orientation;
  GamePageArguments({this.key, @required this.orientation});
}
