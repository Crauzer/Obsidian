import { createContext, useContext } from 'react';

export type WadContextState = {
  wadId: string;
  currentPreviewItemId: string | null;

  changeCurrentPreviewItemId: (itemId: string | null) => void;
};

export const WadContext = createContext<WadContextState>({
  wadId: '',
  currentPreviewItemId: null,
  changeCurrentPreviewItemId: () => {},
});

export const useWadContext = () => useContext(WadContext);
