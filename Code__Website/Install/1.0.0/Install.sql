-- Version 1.0.0

-- Script Settings **********************************************************************************************************************************************************************
SET FOREIGN_KEY_CHECKS = 0;


-- Create tables ************************************************************************************************************************************************************************
-- External Requests
DROP TABLE IF EXISTS `external_requests`;
CREATE TABLE `external_requests` (
  `reason` text,
  `url` text,
  `datetime` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--HTML templates
DROP TABLE IF EXISTS `html_templates`;
CREATE TABLE `html_templates` (
  `hkey` text,
  `html` text,
  `description` text
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Item types
DROP TABLE IF EXISTS `item_types`;
CREATE TABLE `item_types` (
  `typeid` int(11) NOT NULL AUTO_INCREMENT,
  `title` text,
  `uid` int(11) DEFAULT NULL,
  `extensions` text,
  `thumbnail` int(1) NOT NULL DEFAULT '0',
  `interface` text,
  `system` int(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`typeid`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=latin1;

-- Physical folders
DROP TABLE IF EXISTS `physical_folders`;
CREATE TABLE `physical_folders` (
  `pfolderid` int(11) NOT NULL AUTO_INCREMENT,
  `title` text,
  `physicalpath` text NOT NULL,
  `allow_web_synopsis` int(1) NOT NULL,
  PRIMARY KEY (`pfolderid`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;

-- Physical folder types - joining physical folders and item_types
DROP TABLE IF EXISTS `physical_folder_types`;
CREATE TABLE `physical_folder_types` (
  `pfolderid` int(11) NOT NULL DEFAULT '0',
  `typeid` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`pfolderid`,`typeid`),
  KEY `fr_typeid` (`typeid`),
  CONSTRAINT `fr_pfolderid` FOREIGN KEY (`pfolderid`) REFERENCES `physical_folders` (`pfolderid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fr_typeid` FOREIGN KEY (`typeid`) REFERENCES `item_types` (`typeid`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Settings
DROP TABLE IF EXISTS `settings`;
CREATE TABLE `settings` (
  `category` text,
  `keyid` text NOT NULL,
  `value` text,
  `description` text,
  PRIMARY KEY (`keyid`(26))
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Tags
DROP TABLE IF EXISTS `tags`;
CREATE TABLE `tags` (
  `tagid` int(11) NOT NULL AUTO_INCREMENT,
  `title` text,
  PRIMARY KEY (`tagid`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=latin1;

-- Tag items
DROP TABLE IF EXISTS `tag_items`;
CREATE TABLE `tag_items` (
  `tagid` int(11) NOT NULL DEFAULT '0',
  `vitemid` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tagid`,`vitemid`),
  KEY `fk_vitemid` (`vitemid`),
  CONSTRAINT `fk_tagid` FOREIGN KEY (`tagid`) REFERENCES `tags` (`tagid`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_vitemid` FOREIGN KEY (`vitemid`) REFERENCES `virtual_items` (`vitemid`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Terminals
DROP TABLE IF EXISTS `terminals`;
CREATE TABLE `terminals` (
  `terminalid` int(11) NOT NULL AUTO_INCREMENT,
  `title` text,
  `tkey` text,
  `status_state` text,
  `status_volume` double DEFAULT NULL,
  `status_volume_muted` int(1) DEFAULT NULL,
  `status_vitemid` int(11) DEFAULT NULL,
  `status_position` int(11) DEFAULT NULL,
  `status_duration` int(11) DEFAULT NULL,
  `status_updated` datetime DEFAULT NULL,
  PRIMARY KEY (`terminalid`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

-- Terminal buffer
DROP TABLE IF EXISTS `terminal_buffer`;
CREATE TABLE `terminal_buffer` (
  `cid` text,
  `command` text,
  `terminalid` int(11) DEFAULT NULL,
  `arguments` text,
  `queue` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Virtual items
DROP TABLE IF EXISTS `virtual_items`;
CREATE TABLE `virtual_items` (
  `vitemid` int(11) NOT NULL AUTO_INCREMENT,
  `pfolderid` int(11) DEFAULT NULL,
  `parent` int(11) DEFAULT NULL,
  `type_uid` int(1) DEFAULT NULL,
  `title` text,
  `cache_rating` int(11) NOT NULL COMMENT '0',
  `description` text,
  `phy_path` text,
  `vir_path` text,
  `views` int(11) NOT NULL DEFAULT '0',
  `date_added` text,
  PRIMARY KEY (`vitemid`),
  KEY `fk_pfolderid` (`pfolderid`),
  CONSTRAINT `fk_pfolderid` FOREIGN KEY (`pfolderid`) REFERENCES `physical_folders` (`pfolderid`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=25745 DEFAULT CHARSET=latin1;

-- Virtual item ratings
DROP TABLE IF EXISTS `vi_ratings`;
CREATE TABLE `vi_ratings` (
  `uid` text NOT NULL,
  `vitemid` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`uid`(18),`vitemid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;


-- Populate default data ****************************************************************************************************************************************************************
-- Settings (CRITICAL)
INSERT INTO `settings` VALUES ('Version', 'major', '1', 'Critical, do not touch!');
INSERT INTO `settings` VALUES ('Version', 'minor', '0', 'Critical, do not touch!');
INSERT INTO `settings` VALUES ('Version', 'build', '0', 'Critical, do not touch!');

-- Item Types
INSERT INTO `item_types` VALUES ('1', 'Video', '1000', 'avi,mkv,mp4,wmv,m2ts,mpg', 'ffmpeg', 'video_wmp', '0');
INSERT INTO `item_types` VALUES ('2', 'Audio', '1200', 'mp3,wma,wav', '', 'video_wmp', '0');
INSERT INTO `item_types` VALUES ('3', 'YouTube', '1300', 'yt', 'youtube', 'youtube', '0');
INSERT INTO `item_types` VALUES ('4', 'Web Link', '1400', null, '', 'browser', '0');
INSERT INTO `item_types` VALUES ('5', 'Virtual Folder', '100', null, '', null, '1');
INSERT INTO `item_types` VALUES ('6', 'Image', '1500', 'png,jpg,jpeg,gif,bmp', 'image', 'images', '0');

-- Settings
INSERT INTO `settings` VALUES ('Third-party', 'rotten_tomatoes_api_key', '', 'Your API key for Rotten Tomatoes to retrieve third-party media information.');
INSERT INTO `settings` VALUES ('Terminals', 'terminals_automatic_register', '1', 'Specifies if terminals can self-register themselves to your media library; this allows easier installation of terminals/media-computers.');
INSERT INTO `settings` VALUES ('Thumbnails', 'thumbnail_height', '90', 'The height of generated thumbnails for media items.');
INSERT INTO `settings` VALUES ('Thumbnails', 'thumbnail_screenshot_media_time', '90', 'The number of seconds from which a thumbnail snapshot should derive from within a media item.');
INSERT INTO `settings` VALUES ('Thumbnails', 'thumbnail_threads', '4', 'The number of threads simultaneously generating thumbnails for media items.');
INSERT INTO `settings` VALUES ('Thumbnails', 'thumbnail_thread_ttl', '40000', 'The maximum amount of time for a thumbnail to generate an image; if exceeded, the thumbnail generation is terminated.');
INSERT INTO `settings` VALUES ('Thumbnails', 'thumbnail_width', '120', 'The width of generated thumbnails for media items.');

-- Tags
INSERT INTO `tags` VALUES ('1', 'Unsorted');
INSERT INTO `tags` VALUES ('2', 'Action');
INSERT INTO `tags` VALUES ('3', 'Adventure');
INSERT INTO `tags` VALUES ('4', 'Comedy');
INSERT INTO `tags` VALUES ('5', 'Crime & Gangs');
INSERT INTO `tags` VALUES ('6', 'Romance');
INSERT INTO `tags` VALUES ('7', 'War');
INSERT INTO `tags` VALUES ('8', 'Horror');
INSERT INTO `tags` VALUES ('9', 'Musicals');
INSERT INTO `tags` VALUES ('10', 'Western');
INSERT INTO `tags` VALUES ('11', 'Technology');
INSERT INTO `tags` VALUES ('12', 'Epic');
INSERT INTO `tags` VALUES ('13', 'African');
INSERT INTO `tags` VALUES ('14', 'Blues');
INSERT INTO `tags` VALUES ('15', 'Caribbean');
INSERT INTO `tags` VALUES ('16', 'Classical');
INSERT INTO `tags` VALUES ('17', 'Folk');
INSERT INTO `tags` VALUES ('18', 'Electronic');
INSERT INTO `tags` VALUES ('19', 'Jazz');
INSERT INTO `tags` VALUES ('20', 'R & B');
INSERT INTO `tags` VALUES ('21', 'Reggae');
INSERT INTO `tags` VALUES ('22', 'Pop');
INSERT INTO `tags` VALUES ('23', 'Rock');