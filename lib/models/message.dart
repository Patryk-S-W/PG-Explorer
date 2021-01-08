import 'dart:typed_data';

class Message {
  String message;
  String time;
  String userID;
  String userName;
  bool greeting;

  Message({
    this.message = "",
    this.userID = "",
    this.userName = "",
    this.time = "N/A",
    this.greeting = false,
  });

  bool isUserMessage(String senderID) => this.userID == senderID;

  Message.fromJson(Map<String, dynamic> json) {
    print("fromJson");
    print(json);
    message = json['message'] ?? "";
    time = json['time'] ?? "N/A";
    userID = json['userID'] ?? "";
    userName = json['userName'] ?? "";
    greeting = json['greeting'] ?? false;
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = new Map<String, dynamic>();
    data['message'] = this.message ?? "";
    data['time'] = this.time ?? "N/A";
    data['userID'] = this.userID ?? "";
    data['userName'] = this.userName ?? "";
    data['greeting'] = this.greeting ?? false;
    return data;
  }
}
