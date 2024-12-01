/*
 Navicat Premium Data Transfer

 Source Server         : MySQL_root1_connect
 Source Server Type    : MySQL
 Source Server Version : 50726 (5.7.26)
 Source Host           : localhost:3306
 Source Schema         : root1

 Target Server Type    : MySQL
 Target Server Version : 50726 (5.7.26)
 File Encoding         : 65001

 Date: 30/11/2023 21:29:42
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS `user`;
CREATE TABLE `user`  (
  `userId` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `userName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `password` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `chrName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `gender` enum('男','女') CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `registrationDate` date NOT NULL,
  `numberOfBooksAvailableBorrowing` int(11) UNSIGNED NOT NULL,
  `numberOfBooksBorrowing` int(11) UNSIGNED NOT NULL,
  `numberOfOverdue` int(11) NOT NULL,
  PRIMARY KEY (`userId`) USING BTREE,
  UNIQUE INDEX `userName`(`userName`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of user
-- ----------------------------
INSERT INTO `user` VALUES ('1000001', 'user001', '97F3C717DA19B4697AE9884E67AABCE6', '李三', '男', '2021-01-01', 10, 2, 0);
INSERT INTO `user` VALUES ('1000002', 'user002', '8B63B2922634FFAEAB2019E500F13DD20', '王四', '女', '2021-02-15', 10, 0, 1);
INSERT INTO `user` VALUES ('1000003', 'user003', 'E16F3B20872B973A50A3C2A1AC996D99', '赵五', '男', '2021-04-10', 10, 0, 0);

-- ----------------------------
-- Triggers structure for table user
-- ----------------------------
DROP TRIGGER IF EXISTS `user_update_cascade`;
delimiter ;;
CREATE TRIGGER `user_update_cascade` AFTER UPDATE ON `user` FOR EACH ROW BEGIN
  UPDATE borrow SET userId = NEW.userId WHERE userId = OLD.userId;
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table user
-- ----------------------------
DROP TRIGGER IF EXISTS `user_delete_cascade`;
delimiter ;;
CREATE TRIGGER `user_delete_cascade` BEFORE DELETE ON `user` FOR EACH ROW BEGIN
  DELETE FROM borrow WHERE userId = OLD.userId;
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
