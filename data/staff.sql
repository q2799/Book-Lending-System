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

 Date: 30/11/2023 21:29:28
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for staff
-- ----------------------------
DROP TABLE IF EXISTS `staff`;
CREATE TABLE `staff`  (
  `staffId` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `staffName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `password` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `chrName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `gender` enum('男','女') CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `hireDate` date NOT NULL,
  `salary` decimal(10, 2) NOT NULL,
  `administrator` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`staffId`) USING BTREE,
  UNIQUE INDEX `staffName`(`staffName`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of staff
-- ----------------------------
INSERT INTO `staff` VALUES ('10001', 'staff10001', 'E100ADC3949BA59ABBE56E057F200F883E', '张三', '男', '2020-01-01', 8000.00, 1);
INSERT INTO `staff` VALUES ('10002', 'staff10002', 'E100ADC3949BA59ABBE56E057F200F883E', '李四', '男', '2020-02-15', 4000.00, 0);
INSERT INTO `staff` VALUES ('10003', 'staff10003', 'E100ADC3949BA59ABBE56E057F200F883E', '王五', '男', '2021-02-10', 3000.00, 0);

SET FOREIGN_KEY_CHECKS = 1;
