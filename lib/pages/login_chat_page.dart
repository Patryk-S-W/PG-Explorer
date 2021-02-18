import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';

import 'package:flutter_svg/svg.dart';
import 'package:provider/provider.dart';
import 'package:auto_route/auto_route.dart';
import 'package:connectivity/connectivity.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:top_snackbar_flutter/top_snack_bar.dart';
import 'package:top_snackbar_flutter/custom_snack_bar.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:flutter_chat/constants.dart';
import 'package:flutter_chat/router/router.gr.dart';
import 'package:flutter_chat/services/app_localizations.dart';
import 'package:flutter_chat/widgets/custom_alert_dialog.dart';

class LoginChatPage extends StatefulWidget {
  LoginChatPage({Key key}) : super(key: key);

  @override
  _LoginChatPageState createState() => _LoginChatPageState();
}

class _LoginChatPageState extends State<LoginChatPage> {
  TextEditingController _usernameController;
  bool _validate = true;
  bool isAlertboxOpened;

  void checkConnectivity() async {
    var connectivityResult = Provider.of<ConnectivityResult>(context);
    var conn = getConnectionValue(connectivityResult);
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
    _usernameController = TextEditingController();
    super.initState();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    checkConnectivity();
    return Scaffold(
      appBar: AppBar(
        title: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Unity'),
            Row(
              children: [
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 9),
                  child: Text('Syntax'),
                ),
                Text('Error', style: TextStyle(color: Colors.red))
              ],
            )
          ],
        ),
      ),
      body: GestureDetector(
        onTap: () {
          FocusScope.of(context).unfocus();
        },
        child: Container(
          alignment: Alignment.center,
          padding: EdgeInsets.symmetric(horizontal: 30.0.r),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Expanded(
                flex: 3,
                child: Center(
                  child: Text('LeChat',
                      style: GoogleFonts.rockSalt(
                        textStyle: TextStyle(
                          color: Colors.white,
                          fontSize: 55.ssp,
                          fontWeight: FontWeight.bold,
                        ),
                      )),
                ),
              ),
              Expanded(
                flex: 2,
                child: SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    mainAxisSize: MainAxisSize.min,
                    children: <Widget>[
                      Container(
                        width: 250.w,
                        child: TextField(
                          controller: _usernameController,
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.black),
                          decoration: InputDecoration(
                            hintText: AppLocalizations.of(context)
                                .translate('username'),
                            hintStyle: TextStyle(color: Colors.grey),
                            errorText: _validate
                                ? null
                                : AppLocalizations.of(context)
                                    .translate('Enter_username'),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.all(
                                Radius.circular(5.0),
                              ),
                            ),
                            filled: true,
                            fillColor: Colors.white,
                            contentPadding: EdgeInsets.all(15.0.r),
                          ),
                        ),
                      ),
                      SizedBox(
                        height: 30.0.r,
                      ),
                      Container(
                        width: 0.4.sw,
                        child: OutlineButton(
                            borderSide: BorderSide(color: Colors.white),
                            child: Text(
                              AppLocalizations.of(context).translate('JOIN'),
                              style: TextStyle(fontWeight: FontWeight.w400),
                            ),
                            onPressed: () {
                              _openChat();
                            }),
                      ),
                      SizedBox(
                        height: 5.0.r,
                      ),
                      Container(
                        width: 0.4.sw,
                        child: OutlineButton(
                            borderSide: BorderSide(color: Colors.red),
                            child: Text(
                              AppLocalizations.of(context).translate('NOTES'),
                              style: TextStyle(fontWeight: FontWeight.w400),
                            ),
                            onPressed: () {
                              ExtendedNavigator.root.push(Routes.notesPage);
                            }),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  _validation() {
    setState(() {
      _usernameController.text.isEmpty ? _validate = false : _validate = true;
    });
    return _validate;
  }

  _openChat() async {
    FocusScope.of(context).unfocus();

    if (_validation()) {
      try {
        await InternetAddress.lookup(SERVER_URL);
      } on SocketException catch (_) {
        showTopSnackBar(
          context,
          Padding(
              padding: EdgeInsets.only(top: 55.0.r),
              child: CustomSnackBar.error(
                message: AppLocalizations.of(context)
                    .translate("No_internet_connectivity"),
              )),
        );
        return;
      }

      Future.delayed(
          Duration(milliseconds: 5),
          () => {
                ExtendedNavigator.root.push(Routes.chatPage,
                    arguments:
                        ChatPageArguments(username: _usernameController.text)),
                _usernameController.clear()
              });
    }
  }
}
