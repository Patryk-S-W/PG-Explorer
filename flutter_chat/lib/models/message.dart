import 'dart:typed_data';

class Message {
  String message;
  List<int> image;
  String time;
  String userID;
  String username;
  bool isGreeting;

  Message({
    this.message = "",
    this.image,
    this.time = "N/A",
    this.userID = "",
    this.username = "",
    this.isGreeting = false,
  });

  bool isUserMessage(String senderID) => this.userID == senderID;
  bool isImage() => this.message == '' && this.image != null;

  Message.fromJson(Map<String, dynamic> json) {
    message = json['message'] ?? "";
    time = json['time'] ?? "N/A";
    userID = json['userID'] ?? "";
    username = json['username'] ?? "";
    isGreeting = json['isGreeting'] ?? false;
    if (json['image'] != null) {
      List<dynamic> temp = json['image'];
      List<int> intList = new List<int>.from(temp);
      image = Uint8List.fromList(intList);
    }
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = new Map<String, dynamic>();
    data['message'] = this.message ?? "";
    data['image'] = this.image ?? null;
    data['time'] = this.time ?? "N/A";
    data['userID'] = this.userID ?? "";
    data['username'] = this.username ?? "";
    data['isGreeting'] = this.isGreeting ?? false;
    return data;
  }
}
