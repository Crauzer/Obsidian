import { listen } from '@tauri-apps/api/event';
import { useEffect, useState } from 'react';

export type UseFileDropParams = {
  onFileDrop?: (filePaths: string[]) => void;
};

export const useFileDrop = ({ onFileDrop }: UseFileDropParams) => {
  const [isDroppingFile, setIsDroppingFile] = useState(false);

  useEffect(() => {
    const unlisten = listen('tauri://file-drop-hover', () => {
      setIsDroppingFile(true);
    });

    return () => {
      unlisten.then((x) => x());
    };
  }, []);

  useEffect(() => {
    const unlisten = listen('tauri://file-drop-cancelled', () => {
      setIsDroppingFile(false);
    });

    return () => {
      unlisten.then((x) => x());
    };
  }, []);

  useEffect(() => {
    const unlisten = listen<string[]>('tauri://file-drop', (event) => {
      setIsDroppingFile(false);
      onFileDrop?.(event.payload);
    });

    return () => {
      unlisten.then((x) => x());
    };
  }, [onFileDrop]);

  return isDroppingFile;
};
