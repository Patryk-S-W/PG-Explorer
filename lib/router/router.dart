import 'package:auto_route/auto_route_annotations.dart';
import 'package:flutter_chat/pages/home_page.dart';

@AdaptiveAutoRouter(routes: <AutoRoute>[
  AutoRoute(page: HomePage, initial: true),
])
class $Router {}