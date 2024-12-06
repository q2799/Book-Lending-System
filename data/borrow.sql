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

 Date: 30/11/2023 21:29:12
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for borrow
-- ----------------------------
DROP TABLE IF EXISTS `borrow`;
CREATE TABLE `borrow`  (
  `borrowId` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `staffId` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `userId` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `bookId` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `borrowDate` date NOT NULL,
  `returnDate` date NOT NULL,
  `borrowedDate` int(11) NOT NULL,
  `overdue` tinyint(1) NOT NULL,
  `overdueDate` int(11) NULL DEFAULT NULL,
  `overdueCost` decimal(10, 2) NULL DEFAULT NULL,
  INDEX `userId`(`userId`) USING BTREE,
  INDEX `bookId`(`bookId`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of borrow
-- ----------------------------
INSERT INTO `borrow` VALUES ('AA10001', '10003', '1000001', 'A10002', '2023-11-15', '2023-12-15', 15, 0, 0, 0.00);
INSERT INTO `borrow` VALUES ('AA10002', '10003', '1000001', 'A10004', '2023-11-20', '2023-12-20', 10, 0, 0, 0.00);

-- ----------------------------
-- Triggers structure for table borrow
-- ----------------------------
DROP TRIGGER IF EXISTS `check_fk`;
delimiter ;;
CREATE TRIGGER `check_fk` BEFORE INSERT ON `borrow` FOR EACH ROW BEGIN
  IF NOT EXISTS(SELECT * FROM user WHERE userId = NEW.userId) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'UserId does not exist';
  END IF;
  IF NOT EXISTS(SELECT * FROM book WHERE bookId = NEW.bookId) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'BookId does not exist';
  END IF;
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
