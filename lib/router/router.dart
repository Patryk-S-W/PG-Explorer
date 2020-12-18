import 'package:auto_route/auto_route_annotations.dart';
import 'package:flutter_chat/pages/chat_page.dart';
import 'package:flutter_chat/pages/home_page.dart';
import 'package:flutter_chat/pages/login_chat_page.dart';

@AdaptiveAutoRouter(routes: <AutoRoute>[
  AutoRoute(page: HomePage, initial: true),
  AutoRoute(page: LoginChatPage),
  AutoRoute(page: ChatPage),
])
class $Router {}
