import 'package:flutter/material.dart';

import 'package:sqflite/sqflite.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:flutter_chat/models/note.dart';
import 'package:flutter_chat/pages/note_details_page.dart';
import 'package:flutter_chat/services/database_service.dart';
import 'package:flutter_chat/services/app_localizations.dart';

class NotesPage extends StatefulWidget {
  @override
  _NotesPageState createState() => _NotesPageState();
}

class _NotesPageState extends State<NotesPage> {
  DatabaseService databaseHelper = DatabaseService();
  List<Note> noteList;
  int count = 0;
  @override
  Widget build(BuildContext context) {
    if (noteList == null) {
      noteList = List<Note>();
      updateListView();
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(AppLocalizations.of(context).translate('Notes')),
      ),
      body: Padding(
          padding: EdgeInsets.symmetric(horizontal: 3.0.w),
          child: getNoteListView()),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.add),
        backgroundColor: Colors.red,
        onPressed: () {
          navigateToDetails(
              Note('', ''), AppLocalizations.of(context).translate('Add_note'));
        },
      ),
    );
  }

  ListView getNoteListView() {
    return ListView.builder(
      itemCount: count,
      itemBuilder: (BuildContext context, int index) {
        return Card(
          color: Colors.white,
          elevation: 2.0,
          child: ListTile(
            title: Text(
              this.noteList[index].title,
              style: TextStyle(color: Colors.black),
            ),
            subtitle: Text(
              this.noteList[index].date,
              style: TextStyle(color: Colors.black),
            ),
            trailing: IconButton(
              icon: Icon(
                Icons.delete,
                color: Colors.black,
              ),
              onPressed: () {
                _delete(context, this.noteList[index]);
              },
            ),
            onTap: () {
              navigateToDetails(this.noteList[index],
                  AppLocalizations.of(context).translate('Edit_note'));
            },
          ),
        );
      },
    );
  }

  void _delete(BuildContext context, Note note) async {
    int result = await databaseHelper.deleteNote(note.id);
    if (result != 0) {
      _showSnackBar(context,
          AppLocalizations.of(context).translate('Note_deleted_successfully'));
      updateListView();
    }
  }

  void _showSnackBar(BuildContext context, String message) {
    final snackBar = SnackBar(
      content: Text(message),
    );
    Scaffold.of(context).showSnackBar(snackBar);
  }

  void navigateToDetails(Note note, String title) async {
    bool result =
        await Navigator.push(context, MaterialPageRoute(builder: (context) {
      return NoteDetailsPage(note, title);
    }));
    if (result == true) {
      updateListView();
    }
  }

  void updateListView() {
    final Future<Database> dbFuture = databaseHelper.initializeDatabase();
    dbFuture.then((database) {
      Future<List<Note>> noteListFuture = databaseHelper.getNoteList();
      noteListFuture.then((noteList) {
        setState(() {
          this.noteList = noteList;
          this.count = noteList.length;
        });
      });
    });
  }
}
