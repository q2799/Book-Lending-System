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
  `gender` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `hireDate` date NOT NULL,
  `salary` decimal(10, 2) NOT NULL,
  `administrator` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`staffId`) USING BTREE,
  UNIQUE INDEX `staffName`(`staffName`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of staff
-- ----------------------------
INSERT INTO `staff` VALUES ('10001', 'staff001', '0X1E0XF80X5D0X990X690XC30XE20X970X130X560X630X900XA20XE70X610XB7', '张三', '男', '2020-01-01', 8000.00, 1);
INSERT INTO `staff` VALUES ('10002', 'staff002', '0XBE0X660XBA0X7A0X150X650X320X9E0X7E0X2A0X670X270X170X4E0X9D0X41', '李四', '男', '2020-02-15', 4000.00, 0);
INSERT INTO `staff` VALUES ('10003', 'staff003', '0XAE00XC0XBC0X1D0XA90XA10X5F0XCA0X2D0X220XCC0XD50X8500XB0X8D0X7E', '王五', '男', '2021-02-10', 3000.00, 0);

SET FOREIGN_KEY_CHECKS = 1;
