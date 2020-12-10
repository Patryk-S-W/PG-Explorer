import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class LoginChatPage extends StatefulWidget {
  LoginChatPage({Key key}) : super(key: key);

  @override
  _LoginChatPageState createState() => _LoginChatPageState();
}

class _LoginChatPageState extends State<LoginChatPage> {
  TextEditingController _usernameController;

  @override
  void initState() {
    _usernameController = TextEditingController();
    super.initState();
  }

  @override
  void dispose() {
    _usernameController.clear();
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
      body: Container(
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
                          'LOGIN',
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
    );
  }

  _openChat() async {
    print(_usernameController.text);
    _usernameController.clear();
  }
}
