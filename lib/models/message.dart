import 'dart:typed_data';

class Message {
  String message;
  String time;
  String userID;
  String username;
  bool greeting;

  Message({
    this.message = "",
    this.time = "N/A",
    this.userID = "",
    this.username = "",
    this.greeting = false,
  });

  bool isUserMessage(String senderID) => this.userID == senderID;

  Message.fromJson(Map<String, dynamic> json) {
    message = json['message'] ?? "";
    time = json['time'] ?? "N/A";
    userID = json['userID'] ?? "";
    username = json['username'] ?? "";
    greeting = json['greeting'] ?? false;
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = new Map<String, dynamic>();
    data['message'] = this.message ?? "";
    data['time'] = this.time ?? "N/A";
    data['userID'] = this.userID ?? "";
    data['username'] = this.username ?? "";
    data['greeting'] = this.greeting ?? false;
    return data;
  }
}
