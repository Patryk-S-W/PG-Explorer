import 'dart:io';

import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_chat/constants.dart';
import 'package:flutter_chat/router/router.gr.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:top_snackbar_flutter/top_snack_bar.dart';
import 'package:top_snackbar_flutter/custom_snack_bar.dart';

class LoginChatPage extends StatefulWidget {
  LoginChatPage({Key key}) : super(key: key);

  @override
  _LoginChatPageState createState() => _LoginChatPageState();
}

class _LoginChatPageState extends State<LoginChatPage> {
  TextEditingController _usernameController;
  bool _validate = true;

  @override
  void initState() {
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
          padding: EdgeInsets.symmetric(horizontal: 30.0),
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
                          fontSize: 55,
                          fontWeight: FontWeight.bold,
                        ),
                      )),
                ),
              ),
              Expanded(
                flex: 2,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  mainAxisSize: MainAxisSize.min,
                  children: <Widget>[
                    TextField(
                      controller: _usernameController,
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Colors.black),
                      decoration: InputDecoration(
                        hintText: 'username',
                        hintStyle: TextStyle(color: Colors.grey),
                        errorText: _validate ? null : 'Enter username',
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.all(
                            Radius.circular(5.0),
                          ),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                        contentPadding: EdgeInsets.all(20.0),
                      ),
                    ),
                    SizedBox(
                      height: 35.0,
                    ),
                    Container(
                      width: MediaQuery.of(context).size.width,
                      child: OutlineButton(
                          borderSide: BorderSide(color: Colors.white),
                          child: Text(
                            'JOIN',
                            style: TextStyle(fontWeight: FontWeight.w400),
                          ),
                          onPressed: () {
                            _openChat();
                          }),
                    ),
                  ],
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
      } on SocketException catch (err) {
        print('==> $err');
        showTopSnackBar(
          context,
          Padding(
              padding: EdgeInsets.only(top: 55.0),
              child: CustomSnackBar.error(
                message: "No internet connectivity",
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
