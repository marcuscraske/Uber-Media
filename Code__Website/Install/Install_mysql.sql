-- Version 1.0.0

-- Drop tables **************************************************************************************************************************************************************************
SET FOREIGN_KEY_CHECKS=0;
-- DROP TABLE IF EXISTS `external_requests`;
DROP TABLE IF EXISTS `html_templates`;
-- DROP TABLE IF EXISTS `item_types`;
-- DROP TABLE IF EXISTS `physical_folders`;
-- DROP TABLE IF EXISTS `physical_folder_types`;
-- DROP TABLE IF EXISTS `settings`;
-- DROP TABLE IF EXISTS `tags`;
-- DROP TABLE IF EXISTS `tag_items`;
-- DROP TABLE IF EXISTS `terminals`;
DROP TABLE IF EXISTS `terminal_buffer`;
-- DROP TABLE IF EXISTS `virtual_items`;
-- DROP TABLE IF EXISTS `film_information`;
-- DROP TABLE IF EXISTS `film_information_providers`;
SET FOREIGN_KEY_CHECKS=1;

-- Create tables ************************************************************************************************************************************************************************
-- External Requests
CREATE TABLE IF NOT EXISTS `external_requests`
(
	`reason` text,
	`url` text,
	`datetime` datetime DEFAULT NULL
);

-- HTML templates
CREATE TABLE IF NOT EXISTS `html_templates`
(
	`hkey` text,
	`html` text,
	`description` text
);

-- Item types
CREATE TABLE IF NOT EXISTS `item_types`
(
	`typeid` INTEGER PRIMARY KEY AUTO_INCREMENT,
	`title` text,
	`uid` int DEFAULT NULL,
	`extensions` text,
	`thumbnail` text,
	`interface` text,
	`system` int(1) NOT NULL DEFAULT '0'
);

-- Physical folders
CREATE TABLE IF NOT EXISTS `physical_folders`
(
	`pfolderid` INTEGER PRIMARY KEY AUTO_INCREMENT,
	`title` text,
	`physicalpath` text NOT NULL,
	`allow_web_synopsis` int(1) NOT NULL
);

-- Physical folder types - joining physical folders and item_types
CREATE TABLE IF NOT EXISTS `physical_folder_types`
(
	`pfolderid` INT NOT NULL,
	`typeid` INT NOT NULL,
	FOREIGN KEY(`pfolderid`) REFERENCES `physical_folders`(`pfolderid`) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY(`typeid`) REFERENCES `item_types`(`typeid`) ON DELETE CASCADE ON UPDATE CASCADE,
	PRIMARY KEY (`pfolderid`, `typeid`)
);

-- Settings
CREATE TABLE IF NOT EXISTS `settings`
(
	`category` text,
	`keyid` VARCHAR(40) NOT NULL,
	`value` text,
	`description` text,
	PRIMARY KEY (`keyid`)
);

-- Tags
CREATE TABLE IF NOT EXISTS `tags`
(
	`tagid` INTEGER PRIMARY KEY AUTO_INCREMENT,
	`title` text
);

-- Virtual items
CREATE TABLE IF NOT EXISTS `virtual_items`
(
	`vitemid` INTEGER PRIMARY KEY AUTO_INCREMENT,
	`pfolderid` INT NOT NULL,
	FOREIGN KEY(`pfolderid`) REFERENCES `physical_folders`(`pfolderid`) ON DELETE CASCADE ON UPDATE CASCADE,
	`parent` int DEFAULT NULL,
	FOREIGN KEY(`parent`) REFERENCES `virtual_items`(`vitemid`) ON DELETE CASCADE ON UPDATE CASCADE,
	`type_uid` int(1) DEFAULT NULL,
	`title` text,
	`cache_rating` int NOT NULL DEFAULT '0',
	`description` text,
	`phy_path` text,
	`views` INT NOT NULL DEFAULT '0',
	`date_added` text,
	`thumbnail_data` blob
);

-- Tag items
CREATE TABLE IF NOT EXISTS `tag_items`
(
	`tagid` INT NOT NULL,
	`vitemid` INT NOT NULL,
	FOREIGN KEY(`tagid`) REFERENCES `tags`(`tagid`) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY(`vitemid`) REFERENCES `virtual_items`(`vitemid`) ON DELETE CASCADE ON UPDATE CASCADE,
	PRIMARY KEY (`tagid`,`vitemid`)
);

-- Terminals
CREATE TABLE IF NOT EXISTS `terminals`
(
	`terminalid` INTEGER PRIMARY KEY AUTO_INCREMENT,
	`title` text,
	`status_state` text,
	`status_volume` double DEFAULT NULL,
	`status_volume_muted` int(1) DEFAULT NULL,
	`status_vitemid` int DEFAULT NULL,
	`status_position` int DEFAULT NULL,
	`status_duration` int DEFAULT NULL,
	`status_updated` datetime DEFAULT NULL
);

-- Terminal buffer
CREATE TABLE IF NOT EXISTS `terminal_buffer`
(
	`cid` BIGINT PRIMARY KEY AUTO_INCREMENT,
	`command` text,
	`terminalid` int DEFAULT NULL,
	FOREIGN KEY(`terminalid`) REFERENCES `terminals`(`terminalid`) ON DELETE CASCADE ON UPDATE CASCADE,
	`arguments` text,
	`queue` int DEFAULT '0'
);

-- Film information
CREATE TABLE IF NOT EXISTS `film_information_providers`
(
	`provid` integer PRIMARY KEY AUTO_INCREMENT,
	`title` text,
	`cache_updated` datetime
);
CREATE TABLE IF NOT EXISTS `film_information`
(
	`infoid` bigint PRIMARY KEY AUTO_INCREMENT,
	`title` text,
	`description` text,
	`provid` int,
	`last_updated` datetime,
	FOREIGN KEY(`provid`) REFERENCES `film_information_providers`(`provid`) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Populate default data ****************************************************************************************************************************************************************
-- Item Types
INSERT IGNORE INTO `item_types` (`typeid`, `title`, `uid`, `extensions`, `thumbnail`, `interface`, `system`) VALUES
	('1', 'Video', '1000', 'avi,mkv,mp4,wmv,m2ts,mpg', 'ffmpeg', 'video_vlc', '0'),
	('2', 'Audio', '1200', 'mp3,wma,wav', 'audio', 'video_vlc', '0'),
	('3', 'YouTube', '1300', 'yt', 'youtube', 'youtube', '0'),
	('4', 'Web Link', '1400', null, '', 'browser', '0'),
	('5', 'Virtual Folder', '100', null, '', null, '1'),
	('6', 'Image', '1500', 'png,jpg,jpeg,gif,bmp', 'image', 'images', '0')
;

-- Settings
INSERT IGNORE INTO `settings` (`category`, `keyid`, `value`, `description`) VALUES
	('Third-party', 'rotten_tomatoes_api_key', '', 'Your API key for Rotten Tomatoes to retrieve third-party media information.'),
	('Terminals', 'terminals_automatic_register', '1', 'Specifies if terminals can self-register themselves to your media library; this allows easier installation of terminals/media-computers.'),
	('Thumbnails', 'thumbnail_height', '90', 'The height of generated thumbnails for media items.'),
	('Thumbnails', 'thumbnail_screenshot_media_time', '90', 'The number of seconds from which a thumbnail snapshot should derive from within a media item.'),
	('Thumbnails', 'thumbnail_threads', '4', 'The number of threads simultaneously generating thumbnails for media items.'),
	('Thumbnails', 'thumbnail_thread_ttl', '40000', 'The maximum amount of time for a thumbnail to generate an image; if exceeded, the thumbnail generation is terminated.'),
	('Thumbnails', 'thumbnail_width', '120', 'The width of generated thumbnails for media items.'),
	('Conversion', 'conversion_threads', '2', 'The number of conversions to take place simultaneously.'),
	('Conversion', 'conversion_timeout', '14400', 'The maximum time for a conversion to successfully complete.')
;

-- Tags
INSERT IGNORE INTO `tags` (`tagid`, `title`) VALUES
	('1', 'Unsorted'),
	('2', 'Action'),
	('3', 'Adventure'),
	('4', 'Comedy'),
	('5', 'Crime & Gangs'),
	('6', 'Romance'),
	('7', 'War'),
	('8', 'Horror'),
	('9', 'Musicals'),
	('10', 'Western'),
	('11', 'Technology'),
	('12', 'Epic'),
	('13', 'African'),
	('14', 'Blues'),
	('15', 'Caribbean'),
	('16', 'Classical'),
	('17', 'Folk'),
	('18', 'Electronic'),
	('19', 'Jazz'),
	('20', 'R & B'),
	('21', 'Reggae'),
	('22', 'Pop'),
	('23', 'Rock')
;