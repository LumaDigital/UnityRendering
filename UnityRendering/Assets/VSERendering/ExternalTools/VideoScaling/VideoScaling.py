import os
import sys
import logging
import subprocess

class VideoScaling(object):

    _SOURCE_DIR_PATH = os.path.dirname(sys.argv[0])
    _FFMPEG_EXE_PATH = os.path.join(_SOURCE_DIR_PATH, "ffmpeg.exe")

    def main(self):

        logging.basicConfig(level=logging.DEBUG, format='%(message)s')
        if len(sys.argv) > 2:
            if not os.path.exists(sys.argv[1]):
                logging.info("Error: " + str(sys.argv[1]) + " does not exist")
                return False
            if not "x" in sys.argv[2]:
                logging.info("Error: " + str(sys.argv[2]) + " is not the correct format, enter the resolution to scale to")
                logging.info("eg: 1920x1080")
                return False
            else:
                height_and_width = sys.argv[2].split("x")
                if not height_and_width[0].isdigit() or not height_and_width[1].isdigit():
                    logging.info("Error: " + str(sys.argv[2]) + " is not the correct format, enter the resolution to scale to")
                    logging.info("eg: 1920x1080")
                    return False


        else:
            logging.info("Error: Add path for video/image file or directory of files and the resolution after")
            logging.info("Example: VideoScaling.py C:\\Work\\VideoFiles\\ 480x360")
            return False

        self.scale_files(str(sys.argv[1]), str(sys.argv[2]))

        return False

    def scale_files(self, path, resolution):
        if os.path.isdir(path):
            logging.info("\nScaling video files in:\n" + path)
            logging.info("\nProgress:\n")

            for subdir, dirs, files in os.walk(path):
                for file in files:
                    new_file_path = subdir + os.sep + file
                    file_extension = file.split(".")[-1]
                    renamed_old_file_path = subdir + os.sep + "CONVERTING." + file_extension
                    os.rename(new_file_path, renamed_old_file_path)
                    if file_extension in ('mp4', 'avi', 'wmv', 'mov', 'png', 'jpeg'):
                        logging.info(file)

                        cmd = [self._FFMPEG_EXE_PATH, "-i", renamed_old_file_path, "-vf", "scale=" + resolution, new_file_path]
                        process = subprocess.Popen(
                            cmd,
                            stdout=subprocess.PIPE,
                            stderr=subprocess.STDOUT)
                        process.communicate()

                        os.remove(renamed_old_file_path)

        elif os.path.isfile(path):
            logging.info("\nScaling video file:\n" + path)

            file_extension = path.split(".")[-1]
            if file_extension in ('mp4', 'avi', 'wmv', 'mov', 'png', 'jpeg'):
                file = path.split(os.sep)[-1]
                dir = path.replace(file, "")
                new_file_path = dir + file
                renamed_old_file_path = dir + "CONVERTING." + file_extension
                os.rename(new_file_path, renamed_old_file_path)

                cmd = [self._FFMPEG_EXE_PATH, "-i", renamed_old_file_path, "-vf", "scale=" + resolution, new_file_path]
                process = subprocess.Popen(
                    cmd,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.STDOUT)
                process.communicate()

                os.remove(renamed_old_file_path)

        logging.info("\n---Converting Complete---")

if __name__ == "__main__":
    program = VideoScaling()
    program.main()