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

 Date: 30/11/2023 21:29:05
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for book
-- ----------------------------
DROP TABLE IF EXISTS `book`;
CREATE TABLE `book`  (
  `bookId` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `isbn` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `title` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `author` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `publisher` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `publicationDate` date NOT NULL,
  `price` decimal(8, 2) NOT NULL,
  `borrowed` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`bookId`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of book
-- ----------------------------
INSERT INTO `book` VALUES ('A10001', '978-7-302-43370-7', 'Java编程思想', 'Bruce Eckel', '机械工业出版社', '2017-01-01', 99.50, 0);
INSERT INTO `book` VALUES ('A10002', '978-7-302-43370-7', 'Java编程思想', 'Bruce Eckel', '机械工业出版社', '2017-01-01', 99.50, 1);
INSERT INTO `book` VALUES ('A10003', '978-7-302-32903-0', '大话数据结构', '程杰', '电子工业出版社', '2019-08-01', 49.99, 0);
INSERT INTO `book` VALUES ('A10004', '978-7-111-59240-8', '高性能MySQL', 'Baron Schwartz / Peter Zaitsev / Vadim Tkachenko', '机械工业出版社', '2013-06-01', 108.00, 1);

-- ----------------------------
-- Triggers structure for table book
-- ----------------------------
DROP TRIGGER IF EXISTS `book_update_cascade`;
delimiter ;;
CREATE TRIGGER `book_update_cascade` AFTER UPDATE ON `book` FOR EACH ROW BEGIN
  UPDATE borrow SET bookId = NEW.bookId WHERE bookId = OLD.bookId;
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table book
-- ----------------------------
DROP TRIGGER IF EXISTS `book_delete_cascade`;
delimiter ;;
CREATE TRIGGER `book_delete_cascade` BEFORE DELETE ON `book` FOR EACH ROW BEGIN
  DELETE FROM borrow WHERE bookId = OLD.bookId;
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
