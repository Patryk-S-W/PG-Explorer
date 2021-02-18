import 'package:flutter/material.dart';

import 'package:intl/intl.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:flutter_chat/models/note.dart';
import 'package:flutter_chat/services/database_service.dart';
import 'package:flutter_chat/services/app_localizations.dart';

class NoteDetailsPage extends StatefulWidget {
  final String appBarTitle;
  final Note note;

  const NoteDetailsPage(this.note, this.appBarTitle);

  @override
  _NoteDetailsPageState createState() =>
      _NoteDetailsPageState(note, appBarTitle);
}

class _NoteDetailsPageState extends State<NoteDetailsPage> {
  final String appBarTitle;
  final Note note;

  DatabaseService databaseHelper = DatabaseService();

  TextEditingController titleController = TextEditingController();
  TextEditingController descriptionController = TextEditingController();

  _NoteDetailsPageState(this.note, this.appBarTitle);

  @override
  Widget build(BuildContext context) {
    titleController.text = note.title;
    descriptionController.text = note.description;

    return Scaffold(
      appBar: AppBar(
        title: Text(appBarTitle),
      ),
      body: Padding(
        padding: EdgeInsets.all(25.0.r),
        child: ListView(
          children: <Widget>[
            Padding(
              padding: EdgeInsets.symmetric(vertical: 20.0.r),
              child: Container(
                color: Colors.white,
                child: TextField(
                  controller: titleController,
                  style: TextStyle(color: Colors.black),
                  onChanged: (value) {
                    updateTitle();
                  },
                  decoration: InputDecoration(
                      hintText: AppLocalizations.of(context).translate('Title'),
                      hintStyle: TextStyle(color: Colors.black),
                      border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10.0))),
                ),
              ),
            ),
            Padding(
              padding: EdgeInsets.symmetric(vertical: 20.0.r),
              child: Container(
                color: Colors.white,
                child: TextField(
                  controller: descriptionController,
                  style: TextStyle(color: Colors.black),
                  onChanged: (value) {
                    updateDescription();
                  },
                  decoration: InputDecoration(
                      hintText:
                          AppLocalizations.of(context).translate('Description'),
                      hintStyle: TextStyle(color: Colors.black),
                      border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10.0))),
                ),
              ),
            ),
            Padding(
              padding: EdgeInsets.symmetric(vertical: 20.0.r),
              child: Row(
                children: <Widget>[
                  Expanded(
                    child: RaisedButton(
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10.0),
                      ),
                      color: Colors.green,
                      child: Text(
                        AppLocalizations.of(context).translate('Save'),
                        textScaleFactor: 1.5,
                      ),
                      onPressed: () {
                        setState(() {
                          _save();
                        });
                      },
                    ),
                  ),
                  SizedBox(
                    width: 10.0,
                  ),
                  Expanded(
                    child: RaisedButton(
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10.0),
                      ),
                      color: Colors.red,
                      child: Text(
                        AppLocalizations.of(context).translate('Delete'),
                        textScaleFactor: 1.5,
                      ),
                      onPressed: () {
                        setState(() {
                          _delete();
                        });
                      },
                    ),
                  )
                ],
              ),
            )
          ],
        ),
      ),
    );
  }

  void updateTitle() {
    note.title = titleController.text;
  }

  void updateDescription() {
    note.description = descriptionController.text;
  }

  void _save() async {
    moveToLastScreen();
    note.date = DateFormat.yMMMd().format(DateTime.now());
    int result;
    if (note.id != null) {
      result = await databaseHelper.updateNote(note);
    } else {
      result = await databaseHelper.insertNote(note);
    }

    if (result != 0) {
      _showAlertDialog(AppLocalizations.of(context).translate('Status'),
          AppLocalizations.of(context).translate('Note_saved_successfully'));
    } else {
      _showAlertDialog(AppLocalizations.of(context).translate('Status'),
          AppLocalizations.of(context).translate('Note_saved_successfully'));
    }
  }

  void _showAlertDialog(String title, String message) {
    AlertDialog alertDialog = AlertDialog(
      title: Text(title),
      content: Text(message),
    );
    showDialog(context: context, builder: (_) => alertDialog);
  }

  void _delete() async {
    moveToLastScreen();
    if (note.id == null) {
      _showAlertDialog(AppLocalizations.of(context).translate('Status'),
          AppLocalizations.of(context).translate('No_note_was_deleted'));
      return;
    }
    int result = await databaseHelper.deleteNote(note.id);
    if (result != 0) {
      _showAlertDialog(AppLocalizations.of(context).translate('Status'),
          AppLocalizations.of(context).translate('Note_deleted_successfully'));
    } else {
      _showAlertDialog(
          AppLocalizations.of(context).translate('Status'),
          AppLocalizations.of(context)
              .translate('Error_occured_while_deleting_note'));
    }
  }

  void moveToLastScreen() {
    Navigator.pop(context, true);
  }
}
