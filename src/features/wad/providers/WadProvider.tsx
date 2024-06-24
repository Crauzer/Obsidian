import { createContext, useContext } from 'react';

export type WadContextState = {
  wadId: string;

  currentPreviewItemId: string | null;
  changeCurrentPreviewItemId: (itemId: string | null) => void;

  navigate: (itemId: string | null) => void;
};

export const WadContext = createContext<WadContextState>({
  wadId: '',
  currentPreviewItemId: null,
  changeCurrentPreviewItemId: () => {},
  navigate: () => {},
});

export const useWadContext = () => useContext(WadContext);
